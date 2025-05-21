using SharedKernel;

namespace Application.Abstractions.Messaging;

public interface IQueryHandler<in TQuery, TResponse>
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}
