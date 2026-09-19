using FluentValidation;

namespace TaskManagementSystem.Modules.Sprints.Features.Sprints.GetSprintById;

public sealed class GetSprintByIdQueryValidator : AbstractValidator<GetSprintByIdQuery>
{
    public GetSprintByIdQueryValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

