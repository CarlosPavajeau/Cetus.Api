using System.Diagnostics;
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
            var stopwatch = Stopwatch.StartNew();

            logger.LogInformation("Processing command {CommandName}", typeof(TCommand).Name);

            var result = await inner.Handle(command, cancellationToken);

            stopwatch.Stop();

            if (result.IsSuccess)
            {
                logger.LogInformation("Completed command {CommandName} in {Elapsed}ms", typeof(TCommand).Name,
                    stopwatch.ElapsedMilliseconds);
            }
            else
            {
                logger.LogError("Completed command {CommandName} with error in {Elapsed}ms", typeof(TCommand).Name,
                    stopwatch.ElapsedMilliseconds);
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
            var stopwatch = Stopwatch.StartNew();

            logger.LogInformation("Processing command {CommandName}", typeof(TCommand).Name);

            var result = await inner.Handle(command, cancellationToken);

            stopwatch.Stop();

            if (result.IsSuccess)
            {
                logger.LogInformation("Completed command {CommandName} in {Elapsed}ms", typeof(TCommand).Name,
                    stopwatch.ElapsedMilliseconds);
            }
            else
            {
                logger.LogError("Completed command {CommandName} with error in {Elapsed}ms", typeof(TCommand).Name,
                    stopwatch.ElapsedMilliseconds);
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
            var stopwatch = Stopwatch.StartNew();

            logger.LogInformation("Processing query {QueryName}", typeof(TQuery).Name);

            var result = await inner.Handle(query, cancellationToken);

            stopwatch.Stop();

            if (result.IsSuccess)
            {
                logger.LogInformation("Completed query {QueryName} in {Elapsed}ms", typeof(TQuery).Name,
                    stopwatch.ElapsedMilliseconds);
            }
            else
            {
                logger.LogError("Completed query {QueryName} with error in {Elapsed}ms", typeof(TQuery).Name,
                    stopwatch.ElapsedMilliseconds);
            }

            return result;
        }
    }
}
