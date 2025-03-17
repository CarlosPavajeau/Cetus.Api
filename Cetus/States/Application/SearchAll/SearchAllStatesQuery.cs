using MediatR;

namespace Cetus.States.Application.SearchAll;

public sealed record SearchAllStatesQuery : IRequest<IEnumerable<StateResponse>>;
