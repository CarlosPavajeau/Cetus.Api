using Application.Abstractions.Messaging;

namespace Application.Products.Variants.Images.Order;

public sealed record OrderVariantImagesCommand(long VariantId, IReadOnlyList<ProductImageResponse> Images) : ICommand;
