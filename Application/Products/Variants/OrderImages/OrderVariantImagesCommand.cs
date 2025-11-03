using Application.Abstractions.Messaging;

namespace Application.Products.Variants.OrderImages;

public sealed record OrderVariantImagesCommand(long VariantId, IReadOnlyList<ProductImageResponse> Images) : ICommand;
