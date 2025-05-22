using Application.Abstractions.Messaging;

namespace Application.Orders.CalculateInsights;

public sealed record CalculateOrdersInsightsQuery(string Month) : IQuery<OrdersInsightsResponse>;
