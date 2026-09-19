using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.Terms.CreateTerm;

public sealed class CreateTermCommandValidator : AbstractValidator<CreateTermCommand>
{
    public CreateTermCommandValidator()
    {
        RuleFor(x => x.ProjectId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

