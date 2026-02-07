using System.Net;
using System.Net.Http.Json;
using Application.Abstractions.Data;
using Application.Customers.Find;
using Application.Customers.SearchAll;
using Application.Orders.Create;
using Bogus;
using Bogus.Extensions.Belgium;
using Cetus.Api.Test.Shared;
using Cetus.Api.Test.Shared.Fakers;
using Cetus.Api.Test.Shared.Helpers;
using Domain.Orders;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;
using Shouldly;

namespace Cetus.Api.Test;

public class CustomersSpec(ApplicationTestCase factory) : ApplicationContextTestCase(factory)
{
    private readonly Faker _faker = new();
    private readonly CreateOrderCustomerFaker _orderCustomerFaker = new();
    private readonly Guid _cityId = Guid.Parse("f97957e9-d820-4858-ac26-b5d03d658370");

    [Fact(DisplayName = "Should find a customer by ID")]
    public async Task ShouldFindCustomerById()
    {
        // Arrange
        string customerId = _faker.Person.NationalNumber();
        var customer = new Customer
        {
            Id = Guid.CreateVersion7(),
            DocumentType = DocumentType.CC,
            DocumentNumber = customerId,
            Name = _faker.Person.FullName,
            Email = _faker.Internet.Email(),
            Phone = _faker.Phone.PhoneNumber("##########")
        };

        var db = Services.GetRequiredService<IApplicationDbContext>();
        await db.Customers.AddAsync(customer);
        await db.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"api/customers/{customerId}");

        // Assert
        response.EnsureSuccessStatusCode();

        var foundCustomer = await response.DeserializeAsync<CustomerResponse>();

        foundCustomer.ShouldNotBeNull();
        foundCustomer.Name.ShouldBe(customer.Name);
        foundCustomer.Email.ShouldBe(customer.Email);
        foundCustomer.Phone.ShouldBe(customer.Phone);
    }

    [Fact(DisplayName = "Should return not found for non-existing customer")]
    public async Task ShouldReturnNotFoundForNonExistingCustomer()
    {
        // Arrange
        string nonExistingId = _faker.Random.Guid().ToString();

        // Act
        var response = await Client.GetAsync($"api/customers/{nonExistingId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Theory(DisplayName = "Should find a customer by phone number")]
    [InlineData("+1#########")]
    [InlineData("############")]
    [InlineData("+57##########")]
    public async Task ShouldFindCustomerByPhoneNumber(string format)
    {
        // Arrange
        string phoneNumber = _faker.Phone.PhoneNumber(format);
        string normalizedPhone = new([.. phoneNumber.Where(char.IsDigit)]);
        var customer = new Customer
        {
            Id = Guid.CreateVersion7(),
            DocumentType = DocumentType.CC,
            DocumentNumber = _faker.Person.NationalNumber(),
            Name = _faker.Person.FullName,
            Email = _faker.Internet.Email(),
            Phone = normalizedPhone
        };

        var db = Services.GetRequiredService<IApplicationDbContext>();
        await db.Customers.AddAsync(customer);
        await db.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"api/customers/by-phone/{phoneNumber}");

        // Assert
        response.EnsureSuccessStatusCode();

        var foundCustomer = await response.DeserializeAsync<CustomerResponse>();

        foundCustomer.ShouldNotBeNull();
        foundCustomer.Name.ShouldBe(customer.Name);
        foundCustomer.Email.ShouldBe(customer.Email);
        foundCustomer.Phone.ShouldBe(customer.Phone);
    }

    [Fact(DisplayName = "Should return all customers with purchase metrics")]
    public async Task ShouldReturnAllCustomersWithMetrics()
    {
        // Arrange
        var db = Services.GetRequiredService<IApplicationDbContext>();
        await CityHelper.CreateIfNotExists(_cityId, db);

        var product = await ProductHelper.CreateProductWithVariant(Client);
        var order = await CreateOrder(product);

        // Act
        var response = await Client.GetAsync("api/customers");

        // Assert
        response.EnsureSuccessStatusCode();

        var result = await response.DeserializeAsync<PagedResult<CustomerSummaryResponse>>();

        result.ShouldNotBeNull();
        result.Items.ShouldNotBeEmpty();

        result.Items.ShouldContain(c => c.Name == order.Customer.Name);

        var customer = result.Items.First(c => c.Name == order.Customer.Name);
        customer.TotalOrders.ShouldBeGreaterThan(0);
        customer.TotalSpent.ShouldBeGreaterThan(0);
        customer.LastPurchase.ShouldNotBeNull();
    }

    [Fact(DisplayName = "Should search customers by name")]
    public async Task ShouldSearchCustomersByName()
    {
        // Arrange
        var db = Services.GetRequiredService<IApplicationDbContext>();
        await CityHelper.CreateIfNotExists(_cityId, db);

        var product = await ProductHelper.CreateProductWithVariant(Client);
        var order = await CreateOrder(product);

        string searchTerm = order.Customer.Name[..3];

        // Act
        var response = await Client.GetAsync($"api/customers?search={searchTerm}");

        // Assert
        response.EnsureSuccessStatusCode();

        var result = await response.DeserializeAsync<PagedResult<CustomerSummaryResponse>>();

        result.ShouldNotBeNull();
        result.Items.ShouldNotBeEmpty();
        result.Items.ShouldAllBe(c => c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
    }

    [Fact(DisplayName = "Should return empty list when no customers match search")]
    public async Task ShouldReturnEmptyWhenNoMatch()
    {
        // Act
        var response = await Client.GetAsync("api/customers?search=ZZZZNONEXISTENT999");

        // Assert
        response.EnsureSuccessStatusCode();

        var result = await response.DeserializeAsync<PagedResult<CustomerSummaryResponse>>();

        result.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Should paginate customers")]
    public async Task ShouldPaginateCustomers()
    {
        // Arrange
        var db = Services.GetRequiredService<IApplicationDbContext>();
        await CityHelper.CreateIfNotExists(_cityId, db);

        var product = await ProductHelper.CreateProductWithVariant(Client);
        await CreateOrder(product);
        await CreateOrder(product);

        // Act
        var response = await Client.GetAsync("api/customers?page=1&pageSize=1");

        // Assert
        response.EnsureSuccessStatusCode();

        var result = await response.DeserializeAsync<PagedResult<CustomerSummaryResponse>>();

        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(1);
        result.PageSize.ShouldBe(1);
        result.Page.ShouldBe(1);
    }

    private async Task<CreateOrderCommand> CreateOrder(CreateProductWithVariantResponse product)
    {
        var newCustomer = _orderCustomerFaker.Generate();
        var newOrderItems = new List<CreateOrderItem>
        {
            new(product.VariantId, 1)
        };
        var shippingInfo = new CreateOrderShipping(
            _faker.Address.FullAddress(),
            _cityId
        );

        var newOrder = new CreateOrderCommand(newOrderItems, newCustomer, shippingInfo);
        var response = await Client.PostAsJsonAsync("api/orders", newOrder);
        response.EnsureSuccessStatusCode();

        return newOrder;
    }
}
