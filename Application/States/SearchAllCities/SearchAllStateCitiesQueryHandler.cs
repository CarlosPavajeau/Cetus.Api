using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.States.SearchAllCities;

internal sealed class
    SearchAllStateCitiesQueryHandler(IApplicationDbContext context)
    : IQueryHandler<SearchAllStateCitiesQuery, IEnumerable<CityResponse>>
{
    public async Task<Result<IEnumerable<CityResponse>>> Handle(SearchAllStateCitiesQuery request,
        CancellationToken cancellationToken)
    {
        var cities = await context.Cities
            .Where(c => c.DeletedAt == null && c.StateId == request.Id)
            .OrderBy(c => c.Name)
            .Select(c => new CityResponse(c.Id, c.Name))
            .ToListAsync(cancellationToken);

        return cities;
    }
}
