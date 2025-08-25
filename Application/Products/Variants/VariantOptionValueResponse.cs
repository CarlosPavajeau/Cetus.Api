namespace Application.Products.Variants;

public sealed record VariantOptionValueResponse(
    long Id,
    string Value,
    long OptionTypeId,
    string OptionTypeName
);
