namespace TaskManagementSystem.Api.Contracts.HR;

public sealed record CreateHolidayRequest(
    string Name,
    string? Description,
    DateTime StartDate,
    DateTime EndDate);

public sealed record UpdateHolidayRequest(
    string Name,
    string? Description,
    DateTime StartDate,
    DateTime EndDate);

public sealed class HolidayResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedByUserId { get; set; }
}

public sealed class HolidayListPageResponse
{
    public IReadOnlyList<HolidayResponse> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}
