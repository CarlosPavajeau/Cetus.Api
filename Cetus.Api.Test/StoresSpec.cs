using Application.Abstractions.Data;
using Application.Stores;
using Cetus.Api.Test.Shared;
using Domain.Stores;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Cetus.Api.Test;

public class StoresSpec(ApplicationTestCase factory) : ApplicationContextTestCase(factory)
{
    [Fact(DisplayName = "Should get a store by domain")]
    public async Task ShouldGetAStoreByDomain()
    {
        // Arrange
        var store = new Store
        {
            Id = Guid.NewGuid(),
            Name = "Test store",
            CustomDomain = "customdomain.com",
            Slug = "custom_domain"
        };

        var db = Services.GetRequiredService<IApplicationDbContext>();

        await db.Stores.AddAsync(store);
        await db.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"api/stores/by-domain/{store.CustomDomain}");

        // Assert
        response.EnsureSuccessStatusCode();

        var storeResponse = await response.DeserializeAsync<StoreResponse>();

        storeResponse.ShouldNotBeNull();
    }

    [Fact(DisplayName = "Should get a store by slug")]
    public async Task ShouldGetAStoreBySlug()
    {
        // Arrange
        var store = new Store
        {
            Id = Guid.NewGuid(),
            Name = "Test store",
            CustomDomain = "customdomain.com",
            Slug = "custom_domain"
        };

        var db = Services.GetRequiredService<IApplicationDbContext>();

        await db.Stores.AddAsync(store);
        await db.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"api/stores/by-slug/{store.Slug}");

        // Assert
        response.EnsureSuccessStatusCode();

        var storeResponse = await response.DeserializeAsync<StoreResponse>();

        storeResponse.ShouldNotBeNull();
    }
}
