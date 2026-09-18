using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Workflows;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Workflows.Features.Schemas.CreateSchema;

namespace TaskManagementSystem.Api.Endpoints.Workflows.Schemas;

/// <summary>
/// POST /workflows/schemas — Creates a new workflow schema.
/// </summary>
public static class CreateSchemaEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder schemas)
    {
        schemas.MapPost("", HandleAsync)
            .RequirePermissionCode(PermissionCodes.Workflows.Create);

        return schemas;
    }

    private static async Task<IResult> HandleAsync(
        CreateSchemaRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateSchemaCommand(request.Name, request.Description, request.TypeId),
            cancellationToken);

        return result.ToHttpResult(schema =>
            Results.Created($"/workflows/schemas/{schema.Id}", WorkflowMapping.MapSchemaDetail(schema)));
    }
}
