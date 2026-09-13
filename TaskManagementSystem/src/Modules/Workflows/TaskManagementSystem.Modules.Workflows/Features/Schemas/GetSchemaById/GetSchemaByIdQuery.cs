using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.Schemas.GetSchemaById;

public sealed record GetSchemaByIdQuery(int SchemaId) : IQuery<Result<SchemaDetailResult>>;

public sealed class GetSchemaByIdQueryHandler(IWorkflowsUnitOfWork unitOfWork)
    : IRequestHandler<GetSchemaByIdQuery, Result<SchemaDetailResult>>
{
    public async Task<Result<SchemaDetailResult>> Handle(
        GetSchemaByIdQuery request,
        CancellationToken cancellationToken)
    {
        var schema = await unitOfWork.Schemas.GetByIdAsync(request.SchemaId, cancellationToken);
        return schema is null
            ? Result.Fail<SchemaDetailResult>(WorkflowsErrors.SchemaNotFound)
            : Result.Ok(SchemaDetailResult.From(schema));
    }
}
