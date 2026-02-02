using System.Net;
using Application.Abstractions.Data;
using Application.Customers.Find;
using Bogus;
using Bogus.Extensions.Belgium;
using Cetus.Api.Test.Shared;
using Domain.Orders;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Cetus.Api.Test;

public class CustomersSpec(ApplicationTestCase factory) : ApplicationContextTestCase(factory)
{
    private readonly Faker _faker = new();

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
}
