using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.Terms.UpdateTerm;

public sealed class UpdateTermCommandValidator : AbstractValidator<UpdateTermCommand>
{
    public UpdateTermCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

