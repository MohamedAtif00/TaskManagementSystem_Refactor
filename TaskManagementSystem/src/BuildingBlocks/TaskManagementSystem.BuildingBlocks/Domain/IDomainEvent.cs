using MediatR;

namespace TaskManagementSystem.BuildingBlocks.Domain;

public interface IDomainEvent : INotification
{
    Guid Id { get; }

    DateTime OccurredOn { get; }
}
