namespace TaskManagementSystem.Api.Endpoints.Workflows.TaskBank;

public static class WorkflowTaskBankEndpoints
{
    public static RouteGroupBuilder MapWorkflowTaskBankEndpoints(this RouteGroupBuilder group)
    {
        var taskBank = group.MapGroup("/task-bank");

        ListTaskBankEndpoint.Map(taskBank);
        CreateTaskBankItemEndpoint.Map(taskBank);
        UpdateTaskBankItemEndpoint.Map(taskBank);
        DeactivateTaskBankItemEndpoint.Map(taskBank);

        return group;
    }
}
