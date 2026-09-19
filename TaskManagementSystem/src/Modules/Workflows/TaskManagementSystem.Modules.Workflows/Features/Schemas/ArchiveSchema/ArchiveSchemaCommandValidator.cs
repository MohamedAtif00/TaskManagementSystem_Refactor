using FluentValidation;

namespace TaskManagementSystem.Modules.Workflows.Features.Schemas.ArchiveSchema;

public sealed class ArchiveSchemaCommandValidator : AbstractValidator<ArchiveSchemaCommand>
{
    public ArchiveSchemaCommandValidator()
    {
        RuleFor(x => x.SchemaId).GreaterThan(0);
    }
}

