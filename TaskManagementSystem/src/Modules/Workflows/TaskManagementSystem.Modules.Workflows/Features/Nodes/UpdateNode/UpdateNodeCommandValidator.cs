using FluentValidation;

namespace TaskManagementSystem.Modules.Workflows.Features.Nodes.UpdateNode;

public sealed class UpdateNodeCommandValidator : AbstractValidator<UpdateNodeCommand>
{
    public UpdateNodeCommandValidator()
    {
        RuleFor(x => x.NodeId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

