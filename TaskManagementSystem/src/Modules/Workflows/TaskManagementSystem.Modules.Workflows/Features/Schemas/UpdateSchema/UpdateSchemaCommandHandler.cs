using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.Schemas.UpdateSchema;

public sealed class UpdateSchemaCommandHandler(IWorkflowsUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSchemaCommand, Result<SchemaDetailResult>>
{
    public async Task<Result<SchemaDetailResult>> Handle(
        UpdateSchemaCommand request,
        CancellationToken cancellationToken)
    {
        var schema = await unitOfWork.Schemas.GetByIdTrackedAsync(request.SchemaId, cancellationToken);
        if (schema is null)
        {
            return Result.Fail<SchemaDetailResult>(WorkflowsErrors.SchemaNotFound);
        }

        if (request.TypeId is int typeId && !await unitOfWork.Schemas.TypeExistsAsync(typeId, cancellationToken))
        {
            return Result.Fail<SchemaDetailResult>(WorkflowsErrors.SchemaTypeNotFound);
        }

        var updateResult = schema.Update(request.Name, request.Description, request.TypeId);
        if (!updateResult.IsSuccess)
        {
            return Result.Fail<SchemaDetailResult>(updateResult.Error);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(SchemaDetailResult.From(schema));
    }
}

