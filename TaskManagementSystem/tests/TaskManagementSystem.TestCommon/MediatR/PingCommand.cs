using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;

namespace TaskManagementSystem.TestCommon.MediatR;

public sealed record PingCommand : ICommand<string>;

public sealed class PingCommandHandler : IRequestHandler<PingCommand, string>
{
    public Task<string> Handle(PingCommand request, CancellationToken cancellationToken) =>
        Task.FromResult("pong");
}
