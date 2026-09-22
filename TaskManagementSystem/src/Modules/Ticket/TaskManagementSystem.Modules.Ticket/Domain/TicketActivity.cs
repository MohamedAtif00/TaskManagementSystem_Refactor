using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Ticket.Domain;

public sealed class TicketActivity : Entity
{
    private TicketActivity()
    {
    }

    internal static TicketActivity CreateForPersistence() => new();

    public int Id { get; internal set; }
    public TicketActivityType Type { get; internal set; }
    public DateTime TimeStamp { get; internal set; }
    public string? AdditionalInfo { get; internal set; }
    public int TicketId { get; internal set; }
    public int? TicketSecondaryId { get; internal set; }
    public int? ActorOneId { get; internal set; }
    public int? ActorTwoId { get; internal set; }

    public static TicketActivity Create(
        int taskId,
        TicketActivityType type,
        string message,
        int? actorOneId,
        DateTime timestamp)
    {
        return new TicketActivity
        {
            TicketId = taskId,
            Type = type,
            AdditionalInfo = message.Trim(),
            ActorOneId = actorOneId,
            TimeStamp = timestamp,
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}
