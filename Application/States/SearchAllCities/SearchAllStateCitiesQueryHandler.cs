using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.States.SearchAllCities;

internal sealed class
    SearchAllStateCitiesQueryHandler : IQueryHandler<SearchAllStateCitiesQuery, IEnumerable<CityResponse>>
{
    private readonly IApplicationDbContext _context;

    public SearchAllStateCitiesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<CityResponse>>> Handle(SearchAllStateCitiesQuery request,
        CancellationToken cancellationToken)
    {
        var cities = await _context.Cities
            .Where(c => c.DeletedAt == null && c.StateId == request.Id)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return Result.Success(cities.Select(c => new CityResponse(c.Id, c.Name)));
    }
}
