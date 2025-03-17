using MediatR;

namespace Cetus.Products.Application.Delete;

public record DeleteProductCommand(Guid Id) : IRequest<bool>;
