namespace AutomatedTaskSystem.Dtos.Tasks;

public class GetRollbackHistoryDto
{
    public List<GetRollbackDto> Rollbacks { get; set; } = new List<GetRollbackDto> { };
    public List<GetIssueDto> Issues { get; set; } = new List<GetIssueDto> { };
}
