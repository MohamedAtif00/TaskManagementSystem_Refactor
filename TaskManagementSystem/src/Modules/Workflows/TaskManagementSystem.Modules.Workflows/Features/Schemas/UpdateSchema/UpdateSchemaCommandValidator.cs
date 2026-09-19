using FluentValidation;

namespace TaskManagementSystem.Modules.Workflows.Features.Schemas.UpdateSchema;

public sealed class UpdateSchemaCommandValidator : AbstractValidator<UpdateSchemaCommand>
{
    public UpdateSchemaCommandValidator()
    {
        RuleFor(x => x.SchemaId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

