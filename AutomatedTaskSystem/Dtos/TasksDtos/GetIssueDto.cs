using AutomatedTaskSystem.Dtos.Common;

namespace AutomatedTaskSystem.Dtos.Tasks;

public class GetIssueDto
{
    public int Id { get; set; }
    public BasicInfoDto Task { get; set; } = new BasicInfoDto { };
    public string? Note { get; set; }
}
