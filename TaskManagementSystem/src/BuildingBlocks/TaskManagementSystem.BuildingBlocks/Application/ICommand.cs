using MediatR;

namespace TaskManagementSystem.BuildingBlocks.Application;

public interface ICommand : IRequest;

public interface ICommand<out TResult> : IRequest<TResult>;

public interface IQuery<out TResult> : IRequest<TResult>;
