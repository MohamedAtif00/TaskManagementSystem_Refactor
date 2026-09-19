using FluentValidation;

namespace TaskManagementSystem.Modules.Workflows.Features.Schemas.CreateSchema;

public sealed class CreateSchemaCommandValidator : AbstractValidator<CreateSchemaCommand>
{
    public CreateSchemaCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

