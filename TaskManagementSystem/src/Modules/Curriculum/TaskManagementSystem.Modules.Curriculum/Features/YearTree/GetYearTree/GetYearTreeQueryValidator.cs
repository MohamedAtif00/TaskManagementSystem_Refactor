using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.YearTree.GetYearTree;

public sealed class GetYearTreeQueryValidator : AbstractValidator<GetYearTreeQuery>
{
    public GetYearTreeQueryValidator() => RuleFor(x => x.YearId).GreaterThan(0);
}

