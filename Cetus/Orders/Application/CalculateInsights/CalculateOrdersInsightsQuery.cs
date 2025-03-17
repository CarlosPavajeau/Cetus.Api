using MediatR;

namespace Cetus.Orders.Application.CalculateInsights;

public sealed record CalculateOrdersInsightsQuery : IRequest<OrdersInsightsResponse>;
