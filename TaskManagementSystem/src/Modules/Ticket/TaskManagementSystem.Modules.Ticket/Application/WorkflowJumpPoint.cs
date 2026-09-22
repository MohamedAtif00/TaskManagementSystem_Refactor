namespace TaskManagementSystem.Modules.Ticket.Application;

public sealed record WorkflowJumpPoint(int StepId, int NodeId, string Label);
