using Application.Abstractions.Messaging;

namespace Application.States.SearchAllCities;

public sealed record SearchAllStateCitiesQuery(Guid Id) : IQuery<IEnumerable<CityResponse>>;
