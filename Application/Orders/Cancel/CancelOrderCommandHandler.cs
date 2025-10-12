using Application.Abstractions.Data;
using Application.Abstractions.MercadoPago;
using Application.Abstractions.Messaging;
using Application.Orders.SearchAll;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.Cancel;

internal sealed class CancelOrderCommandHandler(IApplicationDbContext db, IMercadoPagoClient mercadoPagoClient)
    : ICommandHandler<CancelOrderCommand, OrderResponse>
{
    private const string PaymentStatusInProcess = "in_process";
    private const string PaymentStatusPending = "pending";
    private const string PaymentStatusAuthorized = "authorized";
    private const string PaymentStatusApproved = "approved";

    public async Task<Result<OrderResponse>> Handle(CancelOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await db.Orders
            .FirstOrDefaultAsync(o => o.Id == command.Id, cancellationToken);

        if (order is null)
        {
            return Result.Failure<OrderResponse>(OrderErrors.NotFound(command.Id));
        }

        if (order.Status == OrderStatus.Canceled)
        {
            return Result.Failure<OrderResponse>(OrderErrors.AlreadyCanceled(command.Id));
        }

        if (!string.IsNullOrEmpty(order.TransactionId))
        {
            var paymentResult = await CancelPayment(order, cancellationToken);

            if (paymentResult.IsFailure)
            {
                return Result.Failure<OrderResponse>(paymentResult.Error);
            }

            if (paymentResult.Value > 0)
            {
                order.RefundId = paymentResult.Value.ToString();
            }
        }

        order.Status = OrderStatus.Canceled;

        await db.SaveChangesAsync(cancellationToken);

        return OrderResponse.FromOrder(order);
    }

    private async Task<Result<long>> CancelPayment(Order order, CancellationToken cancellationToken)
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
                => await HandleCancelablePayment(paymentId, cancellationToken),
            PaymentStatusApproved
                => await HandleApprovedPayment(paymentId, cancellationToken),
            _ => Result.Failure<long>(OrderErrors.PaymentCancellationFailed(paymentId))
        };
    }

    private async Task<Result<long>> HandleCancelablePayment(long paymentId, CancellationToken cancellationToken)
    {
        var result = await mercadoPagoClient.CancelPayment(paymentId, string.Empty, cancellationToken);

        return result is null
            ? Result.Failure<long>(OrderErrors.PaymentNotFound(paymentId))
            : Result.Success(0L);
    }

    private async Task<Result<long>> HandleApprovedPayment(long paymentId, CancellationToken cancellationToken)
    {
        var result = await mercadoPagoClient.RefundPayment(paymentId, string.Empty, cancellationToken);

        return result is null
            ? Result.Failure<long>(OrderErrors.PaymentCancellationFailed(paymentId))
            : Result.Success(result.Id ?? 0L);
    }
}
