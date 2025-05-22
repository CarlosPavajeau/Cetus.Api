using Application.Abstractions.Messaging;

namespace Application.States.SearchAll;

public sealed record SearchAllStatesQuery : IQuery<IEnumerable<StateResponse>>;
