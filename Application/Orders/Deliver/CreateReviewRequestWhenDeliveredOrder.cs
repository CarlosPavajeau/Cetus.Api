using Application.Abstractions.Data;
using Domain.Orders;
using Domain.Reviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Orders.Deliver;

internal sealed class CreateReviewRequestWhenDeliveredOrder(
    IApplicationDbContext db,
    ILogger<CreateReviewRequestWhenDeliveredOrder> logger) : IDomainEventHandler<DeliveredOrderDomainEvent>
{
    public async Task Handle(DeliveredOrderDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating review requests for order {OrderId}", domainEvent.Id);

        var items = await db.OrderItems
            .AsNoTracking()
            .Where(o => o.OrderId == domainEvent.Id)
            .ToListAsync(cancellationToken);

        var customerId = await db.Orders
            .AsNoTracking()
            .Where(o => o.Id == domainEvent.Id)
            .Select(o => o.CustomerId)
            .FirstOrDefaultAsync(cancellationToken);

        var reviews = items.Select(item => new ReviewRequest
        {
            Id = Guid.NewGuid(),
            Status = ReviewRequestStatus.Pending,
            Token = GenerateToken(),
            OrderItemId = item.Id,
            CustomerId = customerId!,
            SendAt = DateTime.UtcNow.AddDays(7), // Send review request after 7 days
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        await db.ReviewRequests.AddRangeAsync(reviews, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Review requests created for order {OrderId}", domainEvent.Id);
    }

    private static string GenerateToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("/", "_")
            .Replace("+", "-")
            .Replace("=", "")[..22];
    }
}
