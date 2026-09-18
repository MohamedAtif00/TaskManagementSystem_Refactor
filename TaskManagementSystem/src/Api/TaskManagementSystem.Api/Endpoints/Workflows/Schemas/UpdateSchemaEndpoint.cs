using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Workflows;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Workflows.Features.Schemas.UpdateSchema;

namespace TaskManagementSystem.Api.Endpoints.Workflows.Schemas;

/// <summary>
/// PUT /workflows/schemas/{id} — Updates a workflow schema.
/// </summary>
public static class UpdateSchemaEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder schemas)
    {
        schemas.MapPut("/{id:int}", HandleAsync)
            .RequirePermissionCode(PermissionCodes.Workflows.Update);

        return schemas;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        UpdateSchemaRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateSchemaCommand(id, request.Name, request.Description, request.TypeId),
            cancellationToken);

        return result.ToHttpResult(schema => Results.Ok(WorkflowMapping.MapSchemaDetail(schema)));
    }
}
