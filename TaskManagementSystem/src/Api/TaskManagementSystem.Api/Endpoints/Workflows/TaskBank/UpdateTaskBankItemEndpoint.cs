using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Workflows;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Workflows.Features.TaskBank.UpdateTaskBankItem;

namespace TaskManagementSystem.Api.Endpoints.Workflows.TaskBank;

/// <summary>
/// PUT /workflows/task-bank/{id} — Updates a task bank item.
/// </summary>
public static class UpdateTaskBankItemEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder taskBank)
    {
        taskBank.MapPut("/{id:int}", HandleAsync)
            .RequirePermissionCode(PermissionCodes.Workflows.Update);

        return taskBank;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        UpdateTaskBankItemRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateTaskBankItemCommand(
                id,
                request.Name,
                request.Duration,
                request.Type,
                request.TeamLeaderOnly,
                request.TeamId),
            cancellationToken);

        return result.ToHttpResult(item => Results.Ok(WorkflowMapping.MapTaskBankListItem(item)));
    }
}
