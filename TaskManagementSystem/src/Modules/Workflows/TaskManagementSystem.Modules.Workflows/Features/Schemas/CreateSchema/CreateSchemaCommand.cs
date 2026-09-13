using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;
using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Features.Schemas.CreateSchema;

public sealed record CreateSchemaCommand(
    string Name,
    string Description,
    int? TypeId) : ICommand<Result<SchemaDetailResult>>;

public sealed class CreateSchemaCommandValidator : AbstractValidator<CreateSchemaCommand>
{
    public CreateSchemaCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public sealed class CreateSchemaCommandHandler(IWorkflowsUnitOfWork unitOfWork)
    : IRequestHandler<CreateSchemaCommand, Result<SchemaDetailResult>>
{
    public async Task<Result<SchemaDetailResult>> Handle(
        CreateSchemaCommand request,
        CancellationToken cancellationToken)
    {
        if (request.TypeId is int typeId && !await unitOfWork.Schemas.TypeExistsAsync(typeId, cancellationToken))
        {
            return Result.Fail<SchemaDetailResult>(WorkflowsErrors.SchemaTypeNotFound);
        }

        var createResult = WorkflowSchema.Create(request.Name, request.Description, request.TypeId);
        if (!createResult.IsSuccess)
        {
            return Result.Fail<SchemaDetailResult>(createResult.Error);
        }

        await unitOfWork.Schemas.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok(SchemaDetailResult.From(createResult.Value));
    }
}
