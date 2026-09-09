using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.Holidays.DeleteHoliday;

public sealed record DeleteHolidayCommand(int HolidayId) : ICommand<Result<NoValue>>;

public sealed class DeleteHolidayCommandHandler(IHrUnitOfWork unitOfWork)
    : IRequestHandler<DeleteHolidayCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(
        DeleteHolidayCommand request,
        CancellationToken cancellationToken)
    {
        var holiday = await unitOfWork.Holidays.GetByIdTrackedAsync(request.HolidayId, cancellationToken);
        if (holiday is null)
        {
            return Result.Fail<NoValue>(HrErrors.HolidayNotFound);
        }

        await unitOfWork.Holidays.DeleteAsync(holiday, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok();
    }
}
