using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Workflows.Domain;

public sealed class WorkflowStep : Entity, IAggregateRoot
{
    private WorkflowStep()
    {
    }

    internal static WorkflowStep CreateForPersistence() => new();

    public int Id { get; internal set; }
    public int Order { get; internal set; }
    public int Duration { get; internal set; }
    public int Priority { get; internal set; }
    public bool Archived { get; internal set; }
    public int NodeId { get; internal set; }
    public int TicketBankId { get; internal set; }

    public static Result<WorkflowStep> Create(
        int order,
        int duration,
        int priority,
        int nodeId,
        int taskBankId)
    {
        if (nodeId <= 0)
        {
            return Result.Fail<WorkflowStep>(new ResultError("node_not_found", "Node not found."));
        }

        if (taskBankId <= 0)
        {
            return Result.Fail<WorkflowStep>(new ResultError("ticket_bank_not_found", "Ticket bank item not found."));
        }

        if (order <= 0)
        {
            return Result.Fail<WorkflowStep>(new ResultError("step_invalid_order", "Step order must be greater than zero."));
        }

        if (duration < 0)
        {
            return Result.Fail<WorkflowStep>(new ResultError("step_invalid_duration", "Duration cannot be negative."));
        }

        return Result.Ok(new WorkflowStep
        {
            Order = order,
            Duration = duration,
            Priority = priority,
            NodeId = nodeId,
            TicketBankId = taskBankId,
            Archived = false
        });
    }

    public Result<NoValue> Update(int order, int duration, int priority, int taskBankId)
    {
        if (taskBankId <= 0)
        {
            return Result.Fail<NoValue>(new ResultError("ticket_bank_not_found", "Ticket bank item not found."));
        }

        if (order <= 0)
        {
            return Result.Fail<NoValue>(new ResultError("step_invalid_order", "Step order must be greater than zero."));
        }

        if (duration < 0)
        {
            return Result.Fail<NoValue>(new ResultError("step_invalid_duration", "Duration cannot be negative."));
        }

        Order = order;
        Duration = duration;
        Priority = priority;
        TicketBankId = taskBankId;
        return Result.Ok();
    }

    public void Archive() => Archived = true;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}
