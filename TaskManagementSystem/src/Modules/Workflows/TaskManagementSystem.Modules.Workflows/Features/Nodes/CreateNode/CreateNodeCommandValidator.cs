using FluentValidation;

namespace TaskManagementSystem.Modules.Workflows.Features.Nodes.CreateNode;

public sealed class CreateNodeCommandValidator : AbstractValidator<CreateNodeCommand>
{
    public CreateNodeCommandValidator()
    {
        RuleFor(x => x.SchemaId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

