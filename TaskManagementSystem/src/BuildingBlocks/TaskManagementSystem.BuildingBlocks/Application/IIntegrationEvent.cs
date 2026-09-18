using MediatR;

namespace TaskManagementSystem.BuildingBlocks.Application;

public interface IIntegrationEvent : INotification
{
    Guid Id { get; }

    DateTime OccurredOn { get; }
}
