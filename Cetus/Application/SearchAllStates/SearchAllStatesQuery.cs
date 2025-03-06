using MediatR;

namespace Cetus.Application.SearchAllStates;

public sealed record SearchAllStatesQuery : IRequest<IEnumerable<StateResponse>>;
