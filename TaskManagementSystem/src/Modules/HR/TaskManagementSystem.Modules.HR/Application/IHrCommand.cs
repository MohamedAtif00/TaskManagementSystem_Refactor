using TaskManagementSystem.BuildingBlocks.Application;

namespace TaskManagementSystem.Modules.HR.Application;

public interface IHrCommand;

public interface IHrCommand<out TResult> : ICommand<TResult>, IHrCommand;
