using Application.Abstractions.Data;
using Application.Abstractions.MercadoPago;
using Application.Abstractions.Messaging;
using Domain.Orders;
using Domain.Stores;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.Cancel;

internal sealed class CancelOrderCommandHandler(
    IApplicationDbContext db,
    IMercadoPagoClient mercadoPagoClient,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CancelOrderCommand, SimpleOrderResponse>
{
    private const string PaymentStatusInProcess = "in_process";
    private const string PaymentStatusPending = "pending";
    private const string PaymentStatusAuthorized = "authorized";
    private const string PaymentStatusApproved = "approved";
    private const string PaymentStatusCanceled = "canceled";
    private const string PaymentStatusRefunded = "refunded";

    public async Task<Result<SimpleOrderResponse>> Handle(CancelOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await db.Orders
            .FirstOrDefaultAsync(o => o.Id == command.Id, cancellationToken);

        if (order is null)
        {
            return Result.Failure<SimpleOrderResponse>(OrderErrors.NotFound(command.Id));
        }

        if (order.Status == OrderStatus.Canceled)
        {
            return Result.Failure<SimpleOrderResponse>(OrderErrors.AlreadyCanceled(command.Id));
        }

        if (!string.IsNullOrEmpty(order.TransactionId))
        {
            var store = await db.Stores
                .AsNoTracking()
                .Where(s => s.Id == order.StoreId)
                .FirstOrDefaultAsync(cancellationToken);

            if (store is null)
            {
                return Result.Failure<SimpleOrderResponse>(StoreErrors.NotFoundById(order.StoreId));
            }

            if (!store.IsConnectedToMercadoPago)
            {
                return Result.Failure<SimpleOrderResponse>(StoreErrors.NotConnectedToMercadoPago(store.Slug));
            }

            var paymentResult = await CancelPayment(order, store.MercadoPagoAccessToken!, cancellationToken);

            if (paymentResult.IsFailure)
            {
                return Result.Failure<SimpleOrderResponse>(paymentResult.Error);
            }

            if (paymentResult.Value > 0)
            {
                order.RefundId = paymentResult.Value.ToString();
            }
        }

        order.Status = OrderStatus.Canceled;
        order.CancellationReason = command.Reason;
        order.CancelledAt = dateTimeProvider.UtcNow;

        order.Raise(new CanceledOrderDomainEvent(order.Id));

        await db.SaveChangesAsync(cancellationToken);

        return SimpleOrderResponse.From(order);
    }

    private async Task<Result<long>> CancelPayment(Order order, string accessToken, CancellationToken cancellationToken)
    {
        if (!long.TryParse(order.TransactionId!, out var paymentId))
        {
            return Result.Failure<long>(OrderErrors.InvalidTransactionId(order.Id));
        }

        var payment = await mercadoPagoClient.FindPaymentById(paymentId, cancellationToken);

        if (payment is null)
        {
            return Result.Failure<long>(OrderErrors.PaymentNotFound(paymentId));
        }

        return payment.Status switch
        {
            PaymentStatusInProcess or PaymentStatusPending or PaymentStatusAuthorized
                => await HandleCancelablePayment(paymentId, accessToken, cancellationToken),
            PaymentStatusApproved
                => await HandleApprovedPayment(paymentId, accessToken, cancellationToken),
            PaymentStatusCanceled or PaymentStatusRefunded
                => Result.Success(0L),
            _ => Result.Failure<long>(OrderErrors.PaymentCancellationFailed(paymentId))
        };
    }

    private async Task<Result<long>> HandleCancelablePayment(long paymentId, string accessToken,
        CancellationToken cancellationToken)
    {
        var result = await mercadoPagoClient.CancelPayment(paymentId, accessToken, cancellationToken);

        return result is null
            ? Result.Failure<long>(OrderErrors.PaymentNotFound(paymentId))
            : Result.Success(0L);
    }

    private async Task<Result<long>> HandleApprovedPayment(long paymentId, string accessToken,
        CancellationToken cancellationToken)
    {
        var result = await mercadoPagoClient.RefundPayment(paymentId, accessToken, cancellationToken);

        return result is null
            ? Result.Failure<long>(OrderErrors.PaymentCancellationFailed(paymentId))
            : Result.Success(result.Id ?? 0L);
    }
}
