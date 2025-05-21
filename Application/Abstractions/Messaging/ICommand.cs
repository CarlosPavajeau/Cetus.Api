using SharedKernel;

namespace Application.Abstractions.Messaging;

public interface ICommand : ICommand<Result>;

public interface ICommand<TResponse> : IBaseCommand;

public interface IBaseCommand;
