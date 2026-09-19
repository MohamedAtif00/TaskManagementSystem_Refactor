using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.Schemas.ListSchemas;

public sealed class ListSchemasQueryHandler(IWorkflowsUnitOfWork unitOfWork)
    : IRequestHandler<ListSchemasQuery, Result<IReadOnlyList<SchemaListItemResult>>>
{
    public async Task<Result<IReadOnlyList<SchemaListItemResult>>> Handle(
        ListSchemasQuery request,
        CancellationToken cancellationToken)
    {
        var schemas = await unitOfWork.Schemas.ListActiveAsync(cancellationToken);
        return Result.Ok<IReadOnlyList<SchemaListItemResult>>(schemas.Select(SchemaListItemResult.From).ToList());
    }
}

