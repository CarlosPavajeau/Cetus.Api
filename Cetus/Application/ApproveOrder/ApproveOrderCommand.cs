using MediatR;

namespace Cetus.Application.ApproveOrder;

public sealed record ApproveOrderCommand(Guid Id) : IRequest<bool>;
