using Application.Abstractions.Messaging;

namespace Application.Orders.Summary;

public sealed record GetOrdersSummaryQuery(string Month) : IQuery<IEnumerable<OrderSummaryResponse>>;
