using System.Collections.Immutable;
using FluentValidation;
using MediatR;

namespace Cetus.Api.Configuration.Validators;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        if (validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task
                .WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)))
                .ConfigureAwait(false);

            var failures = validationResults
                .Where(r => r.Errors.Count > 0)
                .SelectMany(v => v.Errors)
                .ToImmutableList();

            if (failures.Count > 0) throw new ValidationException(failures);
        }

        return await next().ConfigureAwait(false);
    }
}
