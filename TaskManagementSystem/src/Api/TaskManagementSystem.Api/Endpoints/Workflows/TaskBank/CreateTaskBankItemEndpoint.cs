using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Workflows;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Workflows.Features.TaskBank.CreateTaskBankItem;

namespace TaskManagementSystem.Api.Endpoints.Workflows.TaskBank;

/// <summary>
/// POST /workflows/task-bank — Creates a task bank item.
/// </summary>
public static class CreateTaskBankItemEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder taskBank)
    {
        taskBank.MapPost("", HandleAsync)
            .RequirePermissionCode(PermissionCodes.Workflows.Create);

        return taskBank;
    }

    private static async Task<IResult> HandleAsync(
        CreateTaskBankItemRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateTaskBankItemCommand(
                request.Name,
                request.Duration,
                request.Type,
                request.TeamLeaderOnly,
                request.TeamId),
            cancellationToken);

        return result.ToHttpResult(item =>
            Results.Created($"/workflows/task-bank/{item.Id}", WorkflowMapping.MapTaskBankListItem(item)));
    }
}
