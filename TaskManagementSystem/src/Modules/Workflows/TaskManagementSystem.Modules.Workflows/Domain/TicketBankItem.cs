using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Workflows.Domain;

public sealed class TicketBankItem : Entity, IAggregateRoot
{
    private TicketBankItem()
    {
    }

    internal static TicketBankItem CreateForPersistence() => new();

    public int Id { get; internal set; }
    public string Name { get; internal set; } = string.Empty;
    public int Duration { get; internal set; }
    public TicketBankType Type { get; internal set; } = TicketBankType.Creation;
    public bool Active { get; internal set; } = true;
    public bool TeamLeaderOnly { get; internal set; }
    public int TeamId { get; internal set; }

    public static Result<TicketBankItem> Create(
        string name,
        int duration,
        TicketBankType type,
        bool teamLeaderOnly,
        int teamId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<TicketBankItem>(new ResultError("ticket_bank_invalid_name", "Ticket bank name is required."));
        }

        if (duration < 0)
        {
            return Result.Fail<TicketBankItem>(new ResultError("ticket_bank_invalid_duration", "Duration cannot be negative."));
        }

        if (teamId <= 0)
        {
            return Result.Fail<TicketBankItem>(new ResultError("team_invalid", "Team is required."));
        }

        return Result.Ok(new TicketBankItem
        {
            Name = name.Trim(),
            Duration = duration,
            Type = type,
            TeamLeaderOnly = teamLeaderOnly,
            TeamId = teamId,
            Active = true
        });
    }

    public Result<NoValue> Update(
        string name,
        int duration,
        TicketBankType type,
        bool teamLeaderOnly,
        int teamId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<NoValue>(new ResultError("ticket_bank_invalid_name", "Ticket bank name is required."));
        }

        if (duration < 0)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_bank_invalid_duration", "Duration cannot be negative."));
        }

        if (teamId <= 0)
        {
            return Result.Fail<NoValue>(new ResultError("team_invalid", "Team is required."));
        }

        Name = name.Trim();
        Duration = duration;
        Type = type;
        TeamLeaderOnly = teamLeaderOnly;
        TeamId = teamId;
        Active = true;
        return Result.Ok();
    }

    public void Deactivate() => Active = false;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}
