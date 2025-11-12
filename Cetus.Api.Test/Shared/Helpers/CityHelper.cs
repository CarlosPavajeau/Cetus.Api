using Application.Abstractions.Data;
using Domain.States;
using Microsoft.EntityFrameworkCore;

namespace Cetus.Api.Test.Shared.Helpers;

public static class CityHelper
{
    public static async Task CreateIfNotExists(Guid cityId, IApplicationDbContext db)
    {
        var alreadyExists = await db.Cities
            .AnyAsync(c => c.Id == cityId);

        if (alreadyExists)
        {
            return;
        }

        var city = new City
        {
            Id = cityId,
            Name = "Test City",
            State = new State
            {
                Id = Guid.NewGuid(),
                Name = "Test State"
            }
        };

        await db.Cities.AddAsync(city);
        await db.SaveChangesAsync();
    }
}
