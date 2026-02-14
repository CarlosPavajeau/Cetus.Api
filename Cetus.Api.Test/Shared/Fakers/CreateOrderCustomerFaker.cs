using Application.Orders.Create;
using Bogus;
using Bogus.Extensions.Belgium;
using Domain.Customers;

namespace Cetus.Api.Test.Shared.Fakers;

public sealed class CreateOrderCustomerFaker : Faker<CreateOrderCustomer>
{
    public CreateOrderCustomerFaker()
    {
        CustomInstantiator(faker => new CreateOrderCustomer(
            faker.Phone.PhoneNumber("##########"),
            faker.Person.FullName,
            faker.Person.Email,
            DocumentType.CC,
            faker.Person.NationalNumber()
        ));
    }
}
