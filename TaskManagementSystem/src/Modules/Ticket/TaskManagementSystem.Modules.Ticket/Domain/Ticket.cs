using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Domain.Events;

namespace TaskManagementSystem.Modules.Ticket.Domain;

public sealed class Ticket : Entity, IAggregateRoot
{
    private Ticket()
    {
    }

    internal static Ticket CreateForPersistence() => new();

    public int Id { get; internal set; }
    public string Name { get; internal set; } = string.Empty;
    public TicketStatus Status { get; internal set; }
    public TicketPriority Priority { get; internal set; }
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

    public static Result<Ticket> Create(
        string name,
        int duration,
        TicketPriority priority,
        int learningObjectiveId,
        int stepId,
        int teamId,
        bool teamLeaderOnly,
        int? userId,
        DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<Ticket>(new ResultError("ticket_invalid_name", "Ticket name is required."));
        }

        if (learningObjectiveId <= 0)
        {
            return Result.Fail<Ticket>(new ResultError("learning_objective_not_found", "Learning objective is required."));
        }

        if (stepId <= 0)
        {
            return Result.Fail<Ticket>(new ResultError("step_not_found", "Workflow step is required."));
        }

        if (teamId <= 0)
        {
            return Result.Fail<Ticket>(new ResultError("team_invalid", "Team is required."));
        }

        if (duration < 0)
        {
            return Result.Fail<Ticket>(new ResultError("ticket_invalid_duration", "Duration cannot be negative."));
        }

        return Result.Ok(new Ticket
        {
            Name = name.Trim(),
            Status = TicketStatus.Backlog,
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

        if (Status is TicketStatus.Done or TicketStatus.Rollback)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_already_completed", "Ticket is already completed."));
        }

        UserId = userId;
        Status = TicketStatus.ToDo;

        AddDomainEvent(new TicketAssignedDomainEvent(Id, userId));
        return Result.Ok();
    }

    public Result<NoValue> Add(int userId)
    {
        if (userId <= 0)
        {
            return Result.Fail<NoValue>(new ResultError("user_not_found", "User is required."));
        }

        if (Archived)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_archived", "Ticket is archived."));
        }

        if (Status != TicketStatus.Backlog)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_cannot_proceed", "Task cannot be added from this column."));
        }

        if (Flagged)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_flagged", "A flagged task cannot be worked on."));
        }

        UserId = userId;
        Status = TicketStatus.ToDo;
        AddDomainEvent(new TicketAssignedDomainEvent(Id, userId));
        return Result.Ok();
    }

    public Result<NoValue> Start(int userId)
    {
        if (userId <= 0)
        {
            return Result.Fail<NoValue>(new ResultError("user_not_found", "User is required."));
        }

        if (Archived)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_archived", "Ticket is archived."));
        }

        if (Status != TicketStatus.ToDo || Pause)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_cannot_proceed", "Task cannot be started from this column."));
        }

        if (Flagged)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_flagged", "A flagged task cannot be worked on."));
        }

        if (UserId is int assignedUserId && assignedUserId != userId)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_unauthorized", "Task is assigned to someone else."));
        }

        UserId ??= userId;
        Attention = false;
        Status = TicketStatus.Doing;
        return Result.Ok();
    }

    public Result<NoValue> Resume(int userId)
    {
        if (Archived)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_archived", "Ticket is archived."));
        }

        if (!Pause || Status != TicketStatus.ToDo)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_cannot_resume", "Task cannot be resumed if status is not To Do."));
        }

        if (Flagged)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_flagged", "A flagged task cannot be worked on."));
        }

        Pause = false;
        if (UserId == userId)
        {
            Attention = false;
            Status = TicketStatus.Doing;
        }

        return Result.Ok();
    }

    public Result<NoValue> ProceedToStep(int? nextStepId)
    {
        if (Archived)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_archived", "Ticket is archived."));
        }

        if (Status == TicketStatus.Done)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_already_completed", "Ticket is already completed."));
        }

        if (nextStepId is null)
        {
            Status = TicketStatus.Done;
            return Result.Ok();
        }

        StepId = nextStepId;
        Status = TicketStatus.Doing;
        return Result.Ok();
    }

    public Result<NoValue> ToggleFlag()
    {
        if (Archived)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_archived", "Ticket is archived."));
        }

        if (Status is TicketStatus.Done or TicketStatus.Rollback)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_already_completed", "Ticket is already completed."));
        }

        if (Flagged)
        {
            Flagged = false;
            Attention = true;
            return Result.Ok();
        }

        Flagged = true;
        Status = TicketStatus.ToDo;
        return Result.Ok();
    }

    public Result<NoValue> PauseWork()
    {
        if (Archived)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_archived", "Ticket is archived."));
        }

        if (Status != TicketStatus.Doing || Pause)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_cannot_pause", "Task with a status other than Doing cannot be paused."));
        }

        Status = TicketStatus.ToDo;
        Pause = true;
        return Result.Ok();
    }

    public Result<NoValue> Rollback(int actorUserId, bool allowOtherAssignee)
    {
        if (Archived)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_archived", "Ticket is archived."));
        }

        if (!IsReview || Status != TicketStatus.Doing)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_cannot_rollback", "Only a review task in Doing can be rolled back."));
        }

        if (UserId is not int assignedUserId || assignedUserId != actorUserId)
        {
            if (!allowOtherAssignee)
            {
                return Result.Fail<NoValue>(new ResultError("ticket_unauthorized", "Task is assigned to someone else."));
            }
        }

        IsRollback = true;
        RollbackCount += 1;
        Status = TicketStatus.Rollback;
        return Result.Ok();
    }

    public Result<NoValue> Complete(int actorUserId, bool allowOtherAssignee)
    {
        if (Archived)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_archived", "Ticket is archived."));
        }

        if (Status != TicketStatus.Doing)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_cannot_complete", "Task may not be completed yet."));
        }

        if (Flagged)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_flagged", "A flagged task cannot be worked on."));
        }

        if (UserId is int assignedUserId && assignedUserId != actorUserId && !allowOtherAssignee)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_unauthorized", "Task is assigned to someone else."));
        }

        Status = TicketStatus.Done;

        if (UserId is int completedForUserId)
        {
            AddDomainEvent(new TicketCompletedDomainEvent(Id, completedForUserId));
        }

        return Result.Ok();
    }

    public Result<NoValue> Skip()
    {
        if (Archived)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_archived", "Ticket is archived."));
        }

        if (Status == TicketStatus.Done)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_already_completed", "Ticket is already completed."));
        }

        Status = TicketStatus.Done;
        Pause = false;

        if (UserId is int assignedUserId)
        {
            AddDomainEvent(new TicketCompletedDomainEvent(Id, assignedUserId));
        }

        return Result.Ok();
    }

    public void PrepareSuccessor(bool teamLeaderOnly, bool isReview, int? fromId)
    {
        if (teamLeaderOnly)
        {
            Status = TicketStatus.ToDo;
        }

        IsReview = isReview;
        if (fromId is int sourceId)
        {
            FromId = sourceId;
            IsRollback = true;
        }
    }

    public void Reactivate() => Status = TicketStatus.ToDo;

    public Result<NoValue> ChangePriority(TicketPriority priority)
    {
        if (Archived)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_archived", "Ticket is archived."));
        }

        if (!Enum.IsDefined(typeof(TicketPriority), priority))
        {
            return Result.Fail<NoValue>(new ResultError("ticket_invalid_priority", "Priority is invalid."));
        }

        Priority = priority;
        return Result.Ok();
    }

    public Result<NoValue> JumpToStep(int stepId)
    {
        if (Archived)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_archived", "Ticket is archived."));
        }

        if (Status == TicketStatus.Done)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_already_completed", "Ticket is already completed."));
        }

        if (stepId <= 0)
        {
            return Result.Fail<NoValue>(new ResultError("step_not_found", "Workflow step is required."));
        }

        StepId = stepId;
        Status = TicketStatus.Doing;
        return Result.Ok();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}
