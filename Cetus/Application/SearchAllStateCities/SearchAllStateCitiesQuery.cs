using MediatR;

namespace Cetus.Application.SearchAllStateCities;

public sealed record SearchAllStateCitiesQuery(Guid Id) : IRequest<IEnumerable<CityResponse>>;
