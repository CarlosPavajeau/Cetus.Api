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
            faker.Random.Guid()
        ));
    }

    public CreateProductCommandFaker WithCategoryId(Guid categoryId)
    {
        RuleFor(x => x.CategoryId, categoryId);
        return this;
    }
}
