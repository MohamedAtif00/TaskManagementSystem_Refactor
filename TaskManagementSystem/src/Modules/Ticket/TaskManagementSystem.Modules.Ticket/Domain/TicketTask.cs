using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Domain.Events;

namespace TaskManagementSystem.Modules.Ticket.Domain;

public sealed class TicketTask : Entity, IAggregateRoot
{
    private TicketTask()
    {
    }

    internal static TicketTask CreateForPersistence() => new();

    public int Id { get; internal set; }
    public string Name { get; internal set; } = string.Empty;
    public TaskStatus Status { get; internal set; }
    public TaskPriority Priority { get; internal set; }
    public int Duration { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public bool Pause { get; internal set; }
    public bool Attention { get; internal set; }
    public bool Flagged { get; internal set; }
    public bool Tl { get; internal set; }
    public bool IsReview { get; internal set; }
    public bool IsRollback { get; internal set; }
    public int RollbackCount { get; internal set; }
    public bool Archived { get; internal set; }
    public int LearningObjectiveId { get; internal set; }
    public int? StepId { get; internal set; }
    public int? UserId { get; internal set; }
    public int? TeamId { get; internal set; }
    public int? FromId { get; internal set; }

    public static Result<TicketTask> Create(
        string name,
        int duration,
        TaskPriority priority,
        int learningObjectiveId,
        int stepId,
        int teamId,
        bool teamLeaderOnly,
        int? userId,
        DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<TicketTask>(new ResultError("ticket_invalid_name", "Ticket name is required."));
        }

        if (learningObjectiveId <= 0)
        {
            return Result.Fail<TicketTask>(new ResultError("learning_objective_not_found", "Learning objective is required."));
        }

        if (stepId <= 0)
        {
            return Result.Fail<TicketTask>(new ResultError("step_not_found", "Workflow step is required."));
        }

        if (teamId <= 0)
        {
            return Result.Fail<TicketTask>(new ResultError("team_invalid", "Team is required."));
        }

        if (duration < 0)
        {
            return Result.Fail<TicketTask>(new ResultError("ticket_invalid_duration", "Duration cannot be negative."));
        }

        return Result.Ok(new TicketTask
        {
            Name = name.Trim(),
            Status = TaskStatus.Backlog,
            Priority = priority,
            Duration = duration,
            CreatedAt = createdAt,
            Pause = false,
            Attention = false,
            Flagged = false,
            Tl = teamLeaderOnly,
            IsReview = false,
            IsRollback = false,
            RollbackCount = 0,
            Archived = false,
            LearningObjectiveId = learningObjectiveId,
            StepId = stepId,
            UserId = userId,
            TeamId = teamId
        });
    }

    public Result<NoValue> Assign(int userId)
    {
        if (userId <= 0)
        {
            return Result.Fail<NoValue>(new ResultError("user_not_found", "User is required."));
        }

        if (Archived)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_archived", "Ticket is archived."));
        }

        if (Status == TaskStatus.Done)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_already_completed", "Ticket is already completed."));
        }

        UserId = userId;
        if (Status == TaskStatus.Backlog)
        {
            Status = TaskStatus.ToDo;
        }

        AddDomainEvent(new TicketAssignedDomainEvent(Id, userId));
        return Result.Ok();
    }

    public Result<NoValue> ProceedToStep(int? nextStepId)
    {
        if (Archived)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_archived", "Ticket is archived."));
        }

        if (Status == TaskStatus.Done)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_already_completed", "Ticket is already completed."));
        }

        if (nextStepId is null)
        {
            Status = TaskStatus.Done;
            return Result.Ok();
        }

        StepId = nextStepId;
        Status = TaskStatus.Doing;
        return Result.Ok();
    }

    public Result<NoValue> ToggleFlag()
    {
        if (Archived)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_archived", "Ticket is archived."));
        }

        Flagged = !Flagged;
        Attention = Flagged;
        return Result.Ok();
    }

    public Result<NoValue> Rollback()
    {
        if (Archived)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_archived", "Ticket is archived."));
        }

        if (Status == TaskStatus.Backlog)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_cannot_rollback", "Backlog tickets cannot be rolled back."));
        }

        IsRollback = true;
        RollbackCount += 1;
        Status = TaskStatus.Rollback;
        return Result.Ok();
    }

    public Result<NoValue> Complete()
    {
        if (Archived)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_archived", "Ticket is archived."));
        }

        if (Status == TaskStatus.Done)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_already_completed", "Ticket is already completed."));
        }

        Status = TaskStatus.Done;

        if (UserId is int assignedUserId)
        {
            AddDomainEvent(new TicketCompletedDomainEvent(Id, assignedUserId));
        }

        return Result.Ok();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}
