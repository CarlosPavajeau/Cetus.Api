using Application.Abstractions.Data;
using Application.Abstractions.MercadoPago;
using Application.Abstractions.Messaging;
using Application.Abstractions.Wompi;
using Domain.Orders;
using Domain.Stores;
using MercadoPago.Resource.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Orders.SearchPayment;

internal sealed class SearchOrderPaymentQueryHandler(
    IApplicationDbContext db,
    IMercadoPagoClient mercadoPagoClient,
    IWompiClient wompiClient,
    ILogger<SearchOrderPaymentQueryHandler> logger)
    : IQueryHandler<SearchOrderPaymentQuery, OrderPaymentResponse>
{
    public async Task<Result<OrderPaymentResponse>> Handle(SearchOrderPaymentQuery query,
        CancellationToken cancellationToken)
    {
        var order = await db.Orders
            .AsNoTracking()
            .Where(o => o.Id == query.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null)
        {
            return Result.Failure<OrderPaymentResponse>(OrderErrors.NotFound(query.Id));
        }

        if (order.PaymentProvider is null)
        {
            return Result.Failure<OrderPaymentResponse>(OrderErrors.NotPaid(query.Id));
        }

        if (order.PaymentProvider == PaymentProvider.Manual)
        {
            return new OrderPaymentResponse(
                PaymentProvider.Manual,
                order.Id.ToString(),
                order.PaymentStatus,
                order.PaymentMethod,
                order.CreatedAt,
                order.CreatedAt
            );
        }

        return order.PaymentProvider switch
        {
            PaymentProvider.MercadoPago => await SearchMercadoPagoPayment(order, cancellationToken),
            PaymentProvider.Wompi => await SearchWompiPayment(order, cancellationToken),
            _ => Result.Failure<OrderPaymentResponse>(OrderErrors.NotPaid(query.Id))
        };
    }

    private async Task<Result<OrderPaymentResponse>> SearchMercadoPagoPayment(Order order,
        CancellationToken cancellationToken)
    {
        if (!long.TryParse(order.TransactionId, out long paymentId))
        {
            return Result.Failure<OrderPaymentResponse>(OrderErrors.InvalidTransactionId(order.Id));
        }

        Payment? payment = null;

        try
        {
            payment = await mercadoPagoClient.FindPaymentById(paymentId, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error searching MercadoPago payment for order {OrderId} with payment id {PaymentId}",
                order.Id, paymentId);
        }

        if (payment is null)
        {
            return Result.Failure<OrderPaymentResponse>(OrderErrors.PaymentSearchFailed(order.Id));
        }

        return new OrderPaymentResponse(
            PaymentProvider.MercadoPago,
            order.TransactionId,
            order.PaymentStatus,
            order.PaymentMethod,
            payment.DateCreated,
            payment.DateApproved
        );
    }

    private async Task<Result<OrderPaymentResponse>> SearchWompiPayment(Order order,
        CancellationToken cancellationToken)
    {
        string? publicKey = await db.Stores
            .Where(s => s.Id == order.StoreId)
            .Select(s => s.WompiPublicKey)
            .FirstOrDefaultAsync(cancellationToken);

        if (publicKey is null || string.IsNullOrWhiteSpace(publicKey))
        {
            return Result.Failure<OrderPaymentResponse>(StoreErrors.WompiPublicKeyNotFound(order.StoreId));
        }

        if (order.TransactionId is null)
        {
            return Result.Failure<OrderPaymentResponse>(OrderErrors.InvalidTransactionId(order.Id));
        }

        var payment = await wompiClient.FindPaymentById(order.TransactionId, cancellationToken);

        if (payment is null)
        {
            return Result.Failure<OrderPaymentResponse>(OrderErrors.PaymentSearchFailed(order.Id));
        }

        return new OrderPaymentResponse(
            PaymentProvider.Wompi,
            order.TransactionId,
            order.PaymentStatus,
            order.PaymentMethod,
            payment.CreatedAt,
            payment.ApprovedAt
        );
    }
}
