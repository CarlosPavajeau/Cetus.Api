using MediatR;

namespace Cetus.Application.DeleteProduct;

public record DeleteProductCommand(Guid Id) : IRequest<bool>;
