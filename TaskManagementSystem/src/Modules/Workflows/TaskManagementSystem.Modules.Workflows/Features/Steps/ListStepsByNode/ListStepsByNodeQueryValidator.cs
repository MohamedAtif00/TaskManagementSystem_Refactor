using FluentValidation;

namespace TaskManagementSystem.Modules.Workflows.Features.Steps.ListStepsByNode;

public sealed class ListStepsByNodeQueryValidator : AbstractValidator<ListStepsByNodeQuery>
{
    public ListStepsByNodeQueryValidator()
    {
        RuleFor(x => x.NodeId).GreaterThan(0);
    }
}

