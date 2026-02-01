using Application.Orders.Create;
using Bogus;
using Bogus.Extensions.Belgium;

namespace Cetus.Api.Test.Shared.Fakers;

public sealed class CreateOrderCustomerFaker : Faker<CreateOrderCustomer>
{
    public CreateOrderCustomerFaker()
    {
        CustomInstantiator(faker => new CreateOrderCustomer(
            faker.Person.NationalNumber(),
            faker.Person.FullName,
            faker.Person.Email,
            faker.Phone.PhoneNumber("##########"),
            faker.Address.FullAddress()
        ));
    }
}
