using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.SchemaTypes.ListSchemaTypes;

public sealed class ListSchemaTypesQueryHandler(IWorkflowsUnitOfWork unitOfWork)
    : IRequestHandler<ListSchemaTypesQuery, Result<IReadOnlyList<SchemaTypeResult>>>
{
    public async Task<Result<IReadOnlyList<SchemaTypeResult>>> Handle(
        ListSchemaTypesQuery request,
        CancellationToken cancellationToken)
    {
        var types = await unitOfWork.SchemaTypes.ListAsync(cancellationToken);
        return Result.Ok<IReadOnlyList<SchemaTypeResult>>(types.Select(SchemaTypeResult.From).ToList());
    }
}

