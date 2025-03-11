using MediatR;

namespace Cetus.Application.CalculateOrdersInsights;

public sealed record CalculateOrdersInsightsQuery : IRequest<OrdersInsightsResponse>;
