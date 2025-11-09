using Application.Abstractions.Messaging;
using Application.Products.Create;

namespace Application.Products.Variants.Images.Add;

public sealed record AddVariantImagesCommand(long Id, IReadOnlyCollection<CreateProductImage> Images) : ICommand;
