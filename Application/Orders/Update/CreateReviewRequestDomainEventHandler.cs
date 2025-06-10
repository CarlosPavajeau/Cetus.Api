using Application.Abstractions.Data;
using Domain.Orders;
using Domain.Reviews;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Orders.Update;

internal sealed class CreateReviewRequestDomainEventHandler(
    IApplicationDbContext context,
    ILogger<CreateReviewRequestDomainEventHandler> logger)
    : IDomainEventHandler<DeliveredOrderDomainEvent>
{
    public async Task Handle(DeliveredOrderDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating review requests for order {OrderNumber}", domainEvent.Order.OrderNumber);

        foreach (var item in domainEvent.Order.Items)
        {
            var reviewRequest = new ReviewRequest
            {
                Id = Guid.NewGuid(),
                Status = ReviewRequestStatus.Pending,
                Token = GenerateToken(),
                OrderItemId = item.Id,
                CustomerId = domainEvent.Order.CustomerId,
                SendAt = DateTime.UtcNow.AddDays(7), // Send review request after 7 days
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await context.ReviewRequests.AddAsync(reviewRequest, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Review requests created for order {OrderNumber}", domainEvent.Order.OrderNumber);
    }

    private static string GenerateToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("/", "_")
            .Replace("+", "-")
            .Replace("=", "")[..22];
    }
}
