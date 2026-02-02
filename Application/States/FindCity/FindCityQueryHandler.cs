using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.States;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.States.FindCity;

internal sealed class FindCityQueryHandler(IApplicationDbContext db) : IQueryHandler<FindCityQuery, CityResponse>
{
    public async Task<Result<CityResponse>> Handle(FindCityQuery query, CancellationToken cancellationToken)
    {
        var city = await db.Cities
            .AsNoTracking()
            .Where(c => c.Id == query.Id)
            .Select(CityResponse.Map)
            .FirstOrDefaultAsync(cancellationToken);

        if (city is null)
        {
            return Result.Failure<CityResponse>(CityErrors.NotFound(query.Id));
        }

        return city;
    }
}
