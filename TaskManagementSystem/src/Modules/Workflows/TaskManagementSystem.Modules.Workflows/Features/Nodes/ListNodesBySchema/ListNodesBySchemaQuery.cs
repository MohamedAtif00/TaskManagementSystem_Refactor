using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.Nodes.ListNodesBySchema;

public sealed record ListNodesBySchemaQuery(int SchemaId) : IQuery<Result<IReadOnlyList<NodeListItemResult>>>;

