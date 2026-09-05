namespace TaskManagementSystem.BuildingBlocks.Application;

public interface IIntegrationEvent
{
    Guid Id { get; }

    DateTime OccurredOn { get; }
}
