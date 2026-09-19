using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.Terms.ListTermsByProject;

public sealed class ListTermsByProjectQueryValidator : AbstractValidator<ListTermsByProjectQuery>
{
    public ListTermsByProjectQueryValidator() => RuleFor(x => x.ProjectId).GreaterThan(0);
}

