using FluentValidation;

namespace TaskManagementSystem.Modules.Workflows.Features.Nodes.ListNodesBySchema;

public sealed class ListNodesBySchemaQueryValidator : AbstractValidator<ListNodesBySchemaQuery>
{
    public ListNodesBySchemaQueryValidator()
    {
        RuleFor(x => x.SchemaId).GreaterThan(0);
    }
}

