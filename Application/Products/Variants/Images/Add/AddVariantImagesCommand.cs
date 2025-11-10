using Application.Abstractions.Messaging;
using Application.Products.Create;

namespace Application.Products.Variants.Images.Add;

public sealed record AddVariantImagesCommandResponse(long Id, IReadOnlyList<ProductImageResponse> Images);

public sealed record AddVariantImagesCommand(long Id, IReadOnlyCollection<CreateProductImage> Images)
    : ICommand<AddVariantImagesCommandResponse>;
