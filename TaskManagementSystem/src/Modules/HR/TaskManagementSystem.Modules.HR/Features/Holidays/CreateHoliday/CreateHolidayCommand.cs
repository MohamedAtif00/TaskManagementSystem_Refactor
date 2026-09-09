using MediatR;
using TaskManagementSystem.Modules.HR.Features.Holidays;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Features.Holidays.CreateHoliday;

public sealed record CreateHolidayCommand(
    int CreatedByUserId,
    string Name,
    string? Description,
    DateTime StartDate,
    DateTime EndDate) : ICommand<Result<HolidayResult>>;

public sealed class CreateHolidayCommandHandler(
    IHrUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<CreateHolidayCommand, Result<HolidayResult>>
{
    public async Task<Result<HolidayResult>> Handle(
        CreateHolidayCommand request,
        CancellationToken cancellationToken)
    {
        var createResult = PublicHoliday.Create(
            request.Name,
            request.Description,
            request.StartDate,
            request.EndDate,
            request.CreatedByUserId,
            timeProvider.GetUtcNow().UtcDateTime);

        if (!createResult.IsSuccess)
        {
            return Result.Fail<HolidayResult>(HrResultMapper.ToApplicationError(createResult.Error));
        }

        await unitOfWork.Holidays.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok(HolidayResult.From(createResult.Value));
    }
}
