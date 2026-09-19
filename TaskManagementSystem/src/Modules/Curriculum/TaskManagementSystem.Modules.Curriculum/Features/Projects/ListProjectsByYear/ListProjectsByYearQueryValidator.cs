using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.Projects.ListProjectsByYear;

public sealed class ListProjectsByYearQueryValidator : AbstractValidator<ListProjectsByYearQuery>
{
    public ListProjectsByYearQueryValidator() => RuleFor(x => x.YearId).GreaterThan(0);
}

