using MediatR;

namespace Cetus.States.Application.SearchAllCities;

public sealed record SearchAllStateCitiesQuery(Guid Id) : IRequest<IEnumerable<CityResponse>>;
