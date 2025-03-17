using Cetus.Api.Test.Shared;
using Cetus.Infrastructure.Persistence.EntityFramework;
using Cetus.States.Application.SearchAll;
using Cetus.States.Application.SearchAllCities;
using Cetus.States.Domain;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Cetus.Api.Test;

public class StatesSpec(ApplicationTestCase factory) : ApplicationContextTestCase(factory)
{
    [Fact(DisplayName = "Should get all states")]
    public async Task ShouldGetAllStates()
    {
        // Arrange
        var state = new State
        {
            Id = Guid.NewGuid(),
            Name = "Test State"
        };

        var context = Services.GetRequiredService<CetusDbContext>();

        await context.States.AddAsync(state);
        await context.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync("api/states");

        // Assert
        response.EnsureSuccessStatusCode();

        var states = await response.DeserializeAsync<IEnumerable<StateResponse>>();

        states.ShouldNotBeEmpty();
    }
    
    [Fact(DisplayName = "Should get all cities from a state")]
    public async Task ShouldGetAllCitiesFromState()
    {
        // Arrange
        var state = new State
        {
            Id = Guid.NewGuid(),
            Name = "Test State"
        };

        var city = new City
        {
            Id = Guid.NewGuid(),
            Name = "Test City",
            StateId = state.Id
        };

        var context = Services.GetRequiredService<CetusDbContext>();

        await context.States.AddAsync(state);
        await context.Cities.AddAsync(city);
        await context.SaveChangesAsync();
        
        // Act
        var response = await Client.GetAsync($"api/states/{state.Id}/cities");

        // Assert
        response.EnsureSuccessStatusCode();

        var cities = await response.DeserializeAsync<IEnumerable<CityResponse>>();

        cities.ShouldNotBeEmpty();
    }
}
