namespace AutomatedTaskSystem.Dtos.TaskLogger;

public class GetTaskLoggerPagedDto
{
    public List<GetTaskLoggerRowDto> Rows { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
}
