using Bogus;
using Bogus.Extensions.Belgium;
using Cetus.Orders.Application.Create;

namespace Cetus.Api.Test.Shared.Fakers;

public sealed class CreateOrderCustomerFaker : Faker<CreateOrderCustomer>
{
    public CreateOrderCustomerFaker()
    {
        CustomInstantiator(faker => new CreateOrderCustomer(
            faker.Person.NationalNumber(),
            faker.Person.FullName,
            faker.Person.Email,
            faker.Person.Phone,
            faker.Address.FullAddress()
        ));
    }
}
