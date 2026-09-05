using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;

namespace TaskManagementSystem.TestCommon.MediatR;

public sealed record FailingCommand : ICommand;

public sealed class FailingCommandHandler : IRequestHandler<FailingCommand>
{
    public Task Handle(FailingCommand request, CancellationToken cancellationToken) =>
        throw new InvalidOperationException("Handler failed intentionally.");
}
