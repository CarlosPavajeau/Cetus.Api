using Application.Abstractions.Messaging;

namespace Application.Products.Variants.Images.Delete;

public sealed record DeleteVariantImageCommand(long VariantId, long ImageId) : ICommand;
