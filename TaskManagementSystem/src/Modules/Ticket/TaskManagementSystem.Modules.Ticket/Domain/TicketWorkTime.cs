using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Ticket.Domain;

public sealed class TicketWorkTime : Entity, IAggregateRoot
{
    private TicketWorkTime()
    {
    }

    internal static TicketWorkTime CreateForPersistence() => new();

    public int Id { get; internal set; }
    public DateTime StartDate { get; internal set; }
    public DateTime? EndDate { get; internal set; }
    public double Duration { get; internal set; }
    public int? EndReason { get; internal set; }
    public int TicketId { get; internal set; }
    public int UserId { get; internal set; }

    public bool IsOpen => EndDate is null;

    public static Result<TicketWorkTime> Start(int taskId, int userId, DateTime startDate)
    {
        if (taskId <= 0)
        {
            return Result.Fail<TicketWorkTime>(new ResultError("ticket_not_found", "Domain.Ticket is required."));
        }

        if (userId <= 0)
        {
            return Result.Fail<TicketWorkTime>(new ResultError("user_not_found", "User is required."));
        }

        return Result.Ok(new TicketWorkTime
        {
            TicketId = taskId,
            UserId = userId,
            StartDate = startDate,
            Duration = 0,
            EndDate = null
        });
    }

    public Result<NoValue> Stop(DateTime endDate)
    {
        if (EndDate is not null)
        {
            return Result.Fail<NoValue>(new ResultError("work_time_already_stopped", "Work time is already stopped."));
        }

        if (endDate < StartDate)
        {
            return Result.Fail<NoValue>(new ResultError("work_time_invalid_end", "End time cannot be before start time."));
        }

        EndDate = endDate;
        Duration = (endDate - StartDate).TotalMinutes;
        return Result.Ok();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}
