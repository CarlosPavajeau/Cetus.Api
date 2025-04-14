using MediatR;

namespace Cetus.Orders.Application.Summary;

public sealed record GetOrdersSummaryQuery(string Month) : IRequest<IEnumerable<OrderSummaryResponse>>;
