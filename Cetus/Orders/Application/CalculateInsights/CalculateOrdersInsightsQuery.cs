using MediatR;

namespace Cetus.Orders.Application.CalculateInsights;

public sealed record CalculateOrdersInsightsQuery(string Month) : IRequest<OrdersInsightsResponse>;
