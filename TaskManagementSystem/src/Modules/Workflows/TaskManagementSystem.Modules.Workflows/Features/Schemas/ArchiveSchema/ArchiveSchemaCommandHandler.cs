using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.Schemas.ArchiveSchema;

public sealed class ArchiveSchemaCommandHandler(IWorkflowsUnitOfWork unitOfWork)
    : IRequestHandler<ArchiveSchemaCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(
        ArchiveSchemaCommand request,
        CancellationToken cancellationToken)
    {
        var schema = await unitOfWork.Schemas.GetByIdTrackedAsync(request.SchemaId, cancellationToken);
        if (schema is null)
        {
            return Result.Fail<NoValue>(WorkflowsErrors.SchemaNotFound);
        }

        schema.Archive();
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok();
    }
}

