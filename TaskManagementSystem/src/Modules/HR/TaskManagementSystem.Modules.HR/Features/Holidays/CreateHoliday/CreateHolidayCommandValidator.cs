using FluentValidation;
using TaskManagementSystem.Modules.HR.Features.Holidays.CreateHoliday;

namespace TaskManagementSystem.Modules.HR.Features.Holidays.CreateHoliday;

public sealed class CreateHolidayCommandValidator : AbstractValidator<CreateHolidayCommand>
{
    public CreateHolidayCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Description).MaximumLength(1000);
        RuleFor(command => command.EndDate).GreaterThanOrEqualTo(command => command.StartDate);
    }
}
