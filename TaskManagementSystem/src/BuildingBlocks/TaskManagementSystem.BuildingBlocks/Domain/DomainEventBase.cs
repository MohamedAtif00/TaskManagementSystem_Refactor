namespace TaskManagementSystem.BuildingBlocks.Domain;

public abstract record DomainEventBase : IDomainEvent
{
    protected DomainEventBase()
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }

    public Guid Id { get; init; }

    public DateTime OccurredOn { get; init; }
}
