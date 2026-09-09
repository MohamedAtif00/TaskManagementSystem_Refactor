using MediatR;
using TaskManagementSystem.Modules.HR.Features.Holidays;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.Holidays.ListHolidays;

public sealed record ListHolidaysQuery(DateTime? FromDate, DateTime? ToDate)
    : IQuery<Result<IReadOnlyList<HolidayResult>>>;

public sealed class ListHolidaysQueryHandler(IHrUnitOfWork unitOfWork)
    : IRequestHandler<ListHolidaysQuery, Result<IReadOnlyList<HolidayResult>>>
{
    public async Task<Result<IReadOnlyList<HolidayResult>>> Handle(
        ListHolidaysQuery request,
        CancellationToken cancellationToken)
    {
        var holidays = await unitOfWork.Holidays.ListAsync(request.FromDate, request.ToDate, cancellationToken);
        return Result.Ok<IReadOnlyList<HolidayResult>>(holidays.Select(HolidayResult.From).ToList());
    }
}
