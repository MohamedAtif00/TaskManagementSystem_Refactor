using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.Schemas.UpdateSchema;

public sealed record UpdateSchemaCommand(
    int SchemaId,
    string Name,
    string Description,
    int? TypeId) : ICommand<Result<SchemaDetailResult>>;

public sealed class UpdateSchemaCommandValidator : AbstractValidator<UpdateSchemaCommand>
{
    public UpdateSchemaCommandValidator()
    {
        RuleFor(x => x.SchemaId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

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
