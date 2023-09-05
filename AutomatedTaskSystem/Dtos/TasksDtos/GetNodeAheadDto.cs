using AutomatedTaskSystem.Dtos.Common;

namespace AutomatedTaskSystem.Dtos.Tasks;

public class GetNodeAheadDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<BasicInfoDto> PreviousNodes { get; set; } = new List<BasicInfoDto> { };
    public List<BasicInfoDto> NextNodes { get; set; } = new List<BasicInfoDto> { };
    public List<GetStepAheadDto> Steps { get; set; } = new List<GetStepAheadDto> { };
    public bool IsComplete { get; set; }
    public int Order { get; set; }
}
