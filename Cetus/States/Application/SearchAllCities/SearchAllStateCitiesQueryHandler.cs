using Cetus.Infrastructure.Persistence.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cetus.States.Application.SearchAllCities;

internal sealed class
    SearchAllStateCitiesQueryHandler : IRequestHandler<SearchAllStateCitiesQuery, IEnumerable<CityResponse>>
{
    private readonly CetusDbContext _context;

    public SearchAllStateCitiesQueryHandler(CetusDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CityResponse>> Handle(SearchAllStateCitiesQuery request,
        CancellationToken cancellationToken)
    {
        var cities = await _context.Cities
            .Where(c => c.DeletedAt == null && c.StateId == request.Id)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return cities.Select(c => new CityResponse(c.Id, c.Name));
    }
}
