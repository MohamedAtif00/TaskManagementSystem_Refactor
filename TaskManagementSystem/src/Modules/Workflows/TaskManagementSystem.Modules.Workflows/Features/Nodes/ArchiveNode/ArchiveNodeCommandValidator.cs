using FluentValidation;

namespace TaskManagementSystem.Modules.Workflows.Features.Nodes.ArchiveNode;

public sealed class ArchiveNodeCommandValidator : AbstractValidator<ArchiveNodeCommand>
{
    public ArchiveNodeCommandValidator()
    {
        RuleFor(x => x.NodeId).GreaterThan(0);
    }
}

