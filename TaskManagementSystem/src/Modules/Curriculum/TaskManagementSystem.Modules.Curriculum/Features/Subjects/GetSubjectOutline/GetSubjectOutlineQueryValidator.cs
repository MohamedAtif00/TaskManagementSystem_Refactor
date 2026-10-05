using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.GetSubjectOutline;

public sealed class GetSubjectOutlineQueryValidator : AbstractValidator<GetSubjectOutlineQuery>
{
    public GetSubjectOutlineQueryValidator() => RuleFor(x => x.SubjectId).GreaterThan(0);
}
