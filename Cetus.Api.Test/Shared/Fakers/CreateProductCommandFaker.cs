using Application.Products.Create;
using Bogus;

namespace Cetus.Api.Test.Shared.Fakers;

public sealed class CreateProductCommandFaker : Faker<CreateProductCommand>
{
    public CreateProductCommandFaker()
    {
        CustomInstantiator(faker => new CreateProductCommand(
            faker.Commerce.ProductName(),
            faker.Commerce.ProductDescription(),
            faker.Random.Decimal(10, 1000),
            faker.Random.Int(10, 1000),
            faker.Image.PicsumUrl(),
            faker.Random.Guid()
        ));
    }

    public CreateProductCommandFaker WithStock(int stock)
    {
        RuleFor(x => x.Stock, stock);
        return this;
    }

    public CreateProductCommandFaker WithCategoryId(Guid categoryId)
    {
        RuleFor(x => x.CategoryId, categoryId);
        return this;
    }
}
