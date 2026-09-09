using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.HR.Domain;

public sealed class PublicHoliday : Entity, IAggregateRoot
{
    private PublicHoliday()
    {
    }

    public int Id { get; internal set; }

    public string Name { get; internal set; } = string.Empty;

    public string? Description { get; internal set; }

    public DateTime StartDate { get; internal set; }

    public DateTime EndDate { get; internal set; }

    public DateTime CreatedAt { get; internal set; }

    public int CreatedByUserId { get; internal set; }

    internal static PublicHoliday CreateForPersistence() => new();

    public static Result<PublicHoliday> Create(
        string name,
        string? description,
        DateTime startDate,
        DateTime endDate,
        int createdByUserId,
        DateTime utcNow)
    {
        var validation = Validate(name, startDate, endDate);
        if (!validation.IsSuccess)
        {
            return Result.Fail<PublicHoliday>(validation.Error);
        }

        return Result.Ok(new PublicHoliday
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            CreatedByUserId = createdByUserId,
            CreatedAt = utcNow
        });
    }

    public Result<NoValue> Update(string name, string? description, DateTime startDate, DateTime endDate)
    {
        var validation = Validate(name, startDate, endDate);
        if (!validation.IsSuccess)
        {
            return Result.Fail<NoValue>(validation.Error);
        }

        Name = name.Trim();
        Description = description?.Trim();
        StartDate = startDate.Date;
        EndDate = endDate.Date;
        return Result.Ok();
    }

    private static Result<NoValue> Validate(string name, DateTime startDate, DateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail<NoValue>(new ResultError("holiday_invalid_name", "Holiday name is required."));
        }

        if (endDate.Date < startDate.Date)
        {
            return Result.Fail<NoValue>(new ResultError("holiday_invalid_dates", "End date must be on or after start date."));
        }

        return Result.Ok();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}
