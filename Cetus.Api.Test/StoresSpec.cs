using System.Net.Http.Json;
using Application.Abstractions.Data;
using Application.Stores;
using Application.Stores.Create;
using Cetus.Api.Test.Shared;
using Domain.Stores;
using Microsoft.EntityFrameworkCore;
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

    [Fact(DisplayName = "Should create a store")]
    public async Task ShouldCreateAStore()
    {
        // Arrange
        var command = new CreateStoreCommand("Test Store", "test_store", "external-id-123");

        // Act
        var response = await Client.PostAsJsonAsync("api/stores", command);

        // Assert
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();
        result.ShouldBeEmpty(); // Expecting no content on successful creation

        var db = Services.GetRequiredService<IApplicationDbContext>();
        var store = await db.Stores.FirstOrDefaultAsync(x => x.Slug == command.Slug);

        store.ShouldNotBeNull();
        store.Name.ShouldBe(command.Name);
        store.Slug.ShouldBe(command.Slug);
        store.ExternalId.ShouldBe(command.ExternalId);
    }

    [Fact(DisplayName = "Should find a store by ID")]
    public async Task ShouldFindAStoreById()
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
        var response = await Client.GetAsync($"api/stores/{store.Id}");

        // Assert
        response.EnsureSuccessStatusCode();

        var storeResponse = await response.DeserializeAsync<StoreResponse>();

        storeResponse.ShouldNotBeNull();
        storeResponse.Id.ShouldBe(store.Id);
        storeResponse.Name.ShouldBe(store.Name);
        storeResponse.CustomDomain.ShouldBe(store.CustomDomain);
        storeResponse.Slug.ShouldBe(store.Slug);
    }
}
