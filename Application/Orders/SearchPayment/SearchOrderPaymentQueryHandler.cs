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
            .Select(o => new {o.Id, o.PaymentProvider, o.TransactionId, o.StoreId})
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null)
        {
            return Result.Failure<OrderPaymentResponse>(OrderErrors.NotFound(query.Id));
        }

        if (order.PaymentProvider is null || order.TransactionId is null)
        {
            return Result.Failure<OrderPaymentResponse>(OrderErrors.NotPaid(query.Id));
        }

        return order.PaymentProvider switch
        {
            PaymentProvider.MercadoPago => await SearchMercadoPagoPayment(order.TransactionId, query.Id,
                cancellationToken),
            PaymentProvider.Wompi => await SearchWompiPayment(order.TransactionId, query.Id, order.StoreId,
                cancellationToken),
            _ => Result.Failure<OrderPaymentResponse>(OrderErrors.NotPaid(query.Id))
        };
    }

    private async Task<Result<OrderPaymentResponse>> SearchMercadoPagoPayment(string transactionId, Guid orderId,
        CancellationToken cancellationToken)
    {
        if (!long.TryParse(transactionId, out var paymentId))
        {
            return Result.Failure<OrderPaymentResponse>(OrderErrors.InvalidTransactionId(orderId));
        }

        Payment? payment = null;

        try
        {
            payment = await mercadoPagoClient.FindPaymentById(paymentId, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error searching MercadoPago payment for order {OrderId} with payment id {PaymentId}",
                orderId, paymentId);
        }

        if (payment is null)
        {
            return Result.Failure<OrderPaymentResponse>(OrderErrors.PaymentSearchFailed(orderId));
        }

        return new OrderPaymentResponse(
            PaymentProvider.MercadoPago,
            transactionId,
            payment.Status,
            payment.PaymentTypeId,
            payment.DateCreated,
            payment.DateApproved
        );
    }

    private async Task<Result<OrderPaymentResponse>> SearchWompiPayment(string transactionId, Guid orderId,
        Guid storeId, CancellationToken cancellationToken)
    {
        var publicKey = await db.Stores
            .Where(s => s.Id == storeId)
            .Select(s => s.WompiPublicKey)
            .FirstOrDefaultAsync(cancellationToken);

        if (publicKey is null || string.IsNullOrWhiteSpace(publicKey))
        {
            return Result.Failure<OrderPaymentResponse>(StoreErrors.WompiPublicKeyNotFound(storeId));
        }

        var payment = await wompiClient.FindPaymentById(transactionId, cancellationToken);

        if (payment is null)
        {
            return Result.Failure<OrderPaymentResponse>(OrderErrors.PaymentSearchFailed(orderId));
        }

        return payment;
    }
}
