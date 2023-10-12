using AutomatedTaskSystem.Dtos.Common;

namespace AutomatedTaskSystem.Dtos.Schema;

public class GetNodePointDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<BasicInfoDto> PreviousNodes { get; set; } = new List<BasicInfoDto> { };
    public List<BasicInfoDto> NextNodes { get; set; } = new List<BasicInfoDto> { };
    public List<GetStepPointDto> Steps { get; set; } = new List<GetStepPointDto> { };
    public bool IsComplete { get; set; }
    public int Order { get; set; }
}
