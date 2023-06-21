using AutomatedTaskSystem.Dtos.Common;

namespace AutomatedTaskSystem.Dtos.Tasks;

public class GetTaskAssignmentDto
{
    public BasicInfoDto? AssignedUser { get; set; }
    public List<BasicInfoDto> AssignableUsers { get; set; } = new List<BasicInfoDto> { };
}
