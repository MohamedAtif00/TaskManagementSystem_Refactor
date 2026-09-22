namespace TaskManagementSystem.Api.Endpoints.Workflows.TicketBank;

public static class WorkflowTicketBankEndpoints
{
    public static RouteGroupBuilder MapWorkflowTicketBankEndpoints(this RouteGroupBuilder group)
    {
        var taskBank = group.MapGroup("/ticket-bank");

        ListTicketBankEndpoint.Map(taskBank);
        CreateTicketBankItemEndpoint.Map(taskBank);
        UpdateTicketBankItemEndpoint.Map(taskBank);
        DeactivateTicketBankItemEndpoint.Map(taskBank);

        return group;
    }
}
