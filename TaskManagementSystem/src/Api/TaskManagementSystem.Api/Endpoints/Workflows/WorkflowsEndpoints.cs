using MediatR;
using TaskManagementSystem.Api.Contracts.Workflows;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Workflows.Domain;
using TaskManagementSystem.Modules.Workflows.Features;
using TaskManagementSystem.Modules.Workflows.Features.Nodes.ArchiveNode;
using TaskManagementSystem.Modules.Workflows.Features.Nodes.CreateNode;
using TaskManagementSystem.Modules.Workflows.Features.Nodes.ListNodesBySchema;
using TaskManagementSystem.Modules.Workflows.Features.Nodes.UpdateNode;
using TaskManagementSystem.Modules.Workflows.Features.Schemas.ArchiveSchema;
using TaskManagementSystem.Modules.Workflows.Features.Schemas.CreateSchema;
using TaskManagementSystem.Modules.Workflows.Features.Schemas.GetSchemaById;
using TaskManagementSystem.Modules.Workflows.Features.Schemas.ListSchemas;
using TaskManagementSystem.Modules.Workflows.Features.Schemas.UpdateSchema;
using TaskManagementSystem.Modules.Workflows.Features.SchemaTypes.ListSchemaTypes;
using TaskManagementSystem.Modules.Workflows.Features.Steps.ArchiveStep;
using TaskManagementSystem.Modules.Workflows.Features.Steps.CreateStep;
using TaskManagementSystem.Modules.Workflows.Features.Steps.ListStepsByNode;
using TaskManagementSystem.Modules.Workflows.Features.Steps.UpdateStep;
using TaskManagementSystem.Modules.Workflows.Features.TaskBank.CreateTaskBankItem;
using TaskManagementSystem.Modules.Workflows.Features.TaskBank.DeactivateTaskBankItem;
using TaskManagementSystem.Modules.Workflows.Features.TaskBank.ListTaskBank;
using TaskManagementSystem.Modules.Workflows.Features.TaskBank.UpdateTaskBankItem;

namespace TaskManagementSystem.Api.Endpoints.Workflows;

public static class WorkflowsEndpoints
{
    public static RouteGroupBuilder MapWorkflowsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/workflows").WithTags("Workflows").RequireAuthorization();

        group.MapGet("/schema-types", ListSchemaTypesAsync);

        var schemas = group.MapGroup("/schemas");
        schemas.MapGet("", ListSchemasAsync);
        schemas.MapGet("/{id:int}", GetSchemaByIdAsync);
        schemas.MapPost("", CreateSchemaAsync);
        schemas.MapPut("/{id:int}", UpdateSchemaAsync);
        schemas.MapDelete("/{id:int}", ArchiveSchemaAsync);
        schemas.MapGet("/{schemaId:int}/nodes", ListNodesBySchemaAsync);
        schemas.MapPost("/{schemaId:int}/nodes", CreateNodeAsync);

        var nodes = group.MapGroup("/nodes");
        nodes.MapPut("/{id:int}", UpdateNodeAsync);
        nodes.MapDelete("/{id:int}", ArchiveNodeAsync);
        nodes.MapGet("/{nodeId:int}/steps", ListStepsByNodeAsync);
        nodes.MapPost("/{nodeId:int}/steps", CreateStepAsync);

        var steps = group.MapGroup("/steps");
        steps.MapPut("/{id:int}", UpdateStepAsync);
        steps.MapDelete("/{id:int}", ArchiveStepAsync);

        var taskBank = group.MapGroup("/task-bank");
        taskBank.MapGet("", ListTaskBankAsync);
        taskBank.MapPost("", CreateTaskBankItemAsync);
        taskBank.MapPut("/{id:int}", UpdateTaskBankItemAsync);
        taskBank.MapDelete("/{id:int}", DeactivateTaskBankItemAsync);

        return group;
    }

    private static async Task<IResult> ListSchemaTypesAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListSchemaTypesQuery(), cancellationToken);
        return result.ToHttpResult(types =>
            Results.Ok(types.Select(MapSchemaType).ToList()));
    }

    private static async Task<IResult> ListSchemasAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListSchemasQuery(), cancellationToken);
        return result.ToHttpResult(schemas =>
            Results.Ok(schemas.Select(MapSchemaListItem).ToList()));
    }

    private static async Task<IResult> GetSchemaByIdAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSchemaByIdQuery(id), cancellationToken);
        return result.ToHttpResult(schema => Results.Ok(MapSchemaDetail(schema)));
    }

    private static async Task<IResult> CreateSchemaAsync(
        CreateSchemaRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateSchemaCommand(request.Name, request.Description, request.TypeId),
            cancellationToken);

        return result.ToHttpResult(schema =>
            Results.Created($"/workflows/schemas/{schema.Id}", MapSchemaDetail(schema)));
    }

    private static async Task<IResult> UpdateSchemaAsync(
        int id,
        UpdateSchemaRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateSchemaCommand(id, request.Name, request.Description, request.TypeId),
            cancellationToken);

        return result.ToHttpResult(schema => Results.Ok(MapSchemaDetail(schema)));
    }

    private static async Task<IResult> ArchiveSchemaAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ArchiveSchemaCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }

    private static async Task<IResult> ListTaskBankAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListTaskBankQuery(), cancellationToken);
        return result.ToHttpResult(items =>
            Results.Ok(items.Select(MapTaskBankListItem).ToList()));
    }

    private static async Task<IResult> CreateTaskBankItemAsync(
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
            Results.Created($"/workflows/task-bank/{item.Id}", MapTaskBankListItem(item)));
    }

    private static async Task<IResult> UpdateTaskBankItemAsync(
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

        return result.ToHttpResult(item => Results.Ok(MapTaskBankListItem(item)));
    }

    private static async Task<IResult> DeactivateTaskBankItemAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeactivateTaskBankItemCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }

    private static async Task<IResult> ListNodesBySchemaAsync(
        int schemaId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListNodesBySchemaQuery(schemaId), cancellationToken);
        return result.ToHttpResult(nodes =>
            Results.Ok(nodes.Select(MapNodeListItem).ToList()));
    }

    private static async Task<IResult> CreateNodeAsync(
        int schemaId,
        CreateNodeRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateNodeCommand(schemaId, request.Name, request.IsStart, request.IsEnd),
            cancellationToken);

        return result.ToHttpResult(node =>
            Results.Created($"/workflows/nodes/{node.Id}", MapNodeListItem(node)));
    }

    private static async Task<IResult> UpdateNodeAsync(
        int id,
        UpdateNodeRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateNodeCommand(id, request.Name, request.IsStart, request.IsEnd),
            cancellationToken);

        return result.ToHttpResult(node => Results.Ok(MapNodeListItem(node)));
    }

    private static async Task<IResult> ArchiveNodeAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ArchiveNodeCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }

    private static async Task<IResult> ListStepsByNodeAsync(
        int nodeId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListStepsByNodeQuery(nodeId), cancellationToken);
        return result.ToHttpResult(steps =>
            Results.Ok(steps.Select(MapStepListItem).ToList()));
    }

    private static async Task<IResult> CreateStepAsync(
        int nodeId,
        CreateStepRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateStepCommand(nodeId, request.TaskBankId, request.Duration, request.Priority),
            cancellationToken);

        return result.ToHttpResult(step =>
            Results.Created($"/workflows/steps/{step.Id}", MapStepListItem(step)));
    }

    private static async Task<IResult> UpdateStepAsync(
        int id,
        UpdateStepRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateStepCommand(id, request.TaskBankId, request.Duration, request.Priority),
            cancellationToken);

        return result.ToHttpResult(step => Results.Ok(MapStepListItem(step)));
    }

    private static async Task<IResult> ArchiveStepAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ArchiveStepCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }

    private static SchemaTypeResponse MapSchemaType(SchemaTypeResult type) =>
        new()
        {
            Id = type.Id,
            Name = type.Name,
            Description = type.Description
        };

    private static SchemaListItemResponse MapSchemaListItem(SchemaListItemResult schema) =>
        new()
        {
            Id = schema.Id,
            Name = schema.Name,
            Description = schema.Description,
            TypeId = schema.TypeId
        };

    private static SchemaDetailResponse MapSchemaDetail(SchemaDetailResult schema) =>
        new()
        {
            Id = schema.Id,
            Name = schema.Name,
            Description = schema.Description,
            TypeId = schema.TypeId
        };

    private static TaskBankListItemResponse MapTaskBankListItem(TaskBankListItemResult item) =>
        new()
        {
            Id = item.Id,
            Name = item.Name,
            Duration = item.Duration,
            Type = item.Type,
            TeamLeaderOnly = item.TeamLeaderOnly,
            TeamId = item.TeamId
        };

    private static NodeListItemResponse MapNodeListItem(NodeListItemResult node) =>
        new()
        {
            Id = node.Id,
            Name = node.Name,
            Order = node.Order,
            IsStart = node.IsStart,
            IsEnd = node.IsEnd,
            SchemaId = node.SchemaId
        };

    private static StepListItemResponse MapStepListItem(StepListItemResult step) =>
        new()
        {
            Id = step.Id,
            Order = step.Order,
            Duration = step.Duration,
            Priority = step.Priority,
            NodeId = step.NodeId,
            TaskBankId = step.TaskBankId
        };
}
