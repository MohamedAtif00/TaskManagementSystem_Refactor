using MediatR;
using TaskManagementSystem.Modules.HR.Features.Holidays;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.Holidays.UpdateHoliday;

public sealed record UpdateHolidayCommand(
    int HolidayId,
    string Name,
    string? Description,
    DateTime StartDate,
    DateTime EndDate) : ICommand<Result<HolidayResult>>;

public sealed class UpdateHolidayCommandHandler(IHrUnitOfWork unitOfWork)
    : IRequestHandler<UpdateHolidayCommand, Result<HolidayResult>>
{
    public async Task<Result<HolidayResult>> Handle(
        UpdateHolidayCommand request,
        CancellationToken cancellationToken)
    {
        var holiday = await unitOfWork.Holidays.GetByIdTrackedAsync(request.HolidayId, cancellationToken);
        if (holiday is null)
        {
            return Result.Fail<HolidayResult>(HrErrors.HolidayNotFound);
        }

        var updateResult = holiday.Update(request.Name, request.Description, request.StartDate, request.EndDate);
        if (!updateResult.IsSuccess)
        {
            return Result.Fail<HolidayResult>(HrResultMapper.ToApplicationError(updateResult.Error));
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok(HolidayResult.From(holiday));
    }
}
