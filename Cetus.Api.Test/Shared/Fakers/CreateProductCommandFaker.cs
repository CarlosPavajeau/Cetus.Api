using Bogus;
using Cetus.Products.Application.Create;

namespace Cetus.Api.Test.Shared.Fakers;

public sealed class CreateProductCommandFaker : Faker<CreateProductCommand>
{
    public CreateProductCommandFaker()
    {
        CustomInstantiator(faker => new CreateProductCommand(
            faker.Commerce.ProductName(),
            faker.Commerce.ProductDescription(),
            faker.Random.Decimal(1, 1000),
            faker.Random.Int(1, 1000),
            faker.Image.PicsumUrl(),
            faker.Random.Guid()
        ));
    }

    public CreateProductCommandFaker WithStock(int stock)
    {
        RuleFor(x => x.Stock, stock);
        return this;
    }
}
