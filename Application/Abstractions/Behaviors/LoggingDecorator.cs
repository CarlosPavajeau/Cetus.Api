using Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Abstractions.Behaviors;

internal static class LoggingDecorator
{
    internal sealed class CommandBaseHandler<TCommand>(
        ICommandHandler<TCommand> inner,
        ILogger<CommandBaseHandler<TCommand>> logger)
        : ICommandHandler<TCommand> where TCommand : ICommand
    {
        public async Task<Result> Handle(TCommand command, CancellationToken cancellationToken)
        {
            logger.LogInformation("Processing command {CommandName}", typeof(TCommand).Name);

            Result result = await inner.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                logger.LogInformation("Completed command {CommandName}", typeof(TCommand).Name);
            }
            else
            {
                logger.LogError("Completed command {CommandName} with error", typeof(TCommand).Name);
            }

            return result;
        }
    }

    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> inner,
        ILogger<CommandHandler<TCommand, TResponse>> logger)
        : ICommandHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
        {
            logger.LogInformation("Processing command {CommandName}", typeof(TCommand).Name);

            Result<TResponse> result = await inner.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                logger.LogInformation("Completed command {CommandName}", typeof(TCommand).Name);
            }
            else
            {
                logger.LogError("Completed command {CommandName} with error", typeof(TCommand).Name);
            }

            return result;
        }
    }

    internal sealed class QueryHandler<TQuery, TResponse>(
        IQueryHandler<TQuery, TResponse> inner,
        ILogger<QueryHandler<TQuery, TResponse>> logger)
        : IQueryHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
        {
            logger.LogInformation("Processing query {QueryName}", typeof(TQuery).Name);

            Result<TResponse> result = await inner.Handle(query, cancellationToken);

            if (result.IsSuccess)
            {
                logger.LogInformation("Completed query {QueryName}", typeof(TQuery).Name);
            }
            else
            {
                logger.LogError("Completed query {QueryName} with error", typeof(TQuery).Name);
            }

            return result;
        }
    }
}
