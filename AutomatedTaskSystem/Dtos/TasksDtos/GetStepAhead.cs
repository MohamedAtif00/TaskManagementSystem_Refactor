using AutomatedTaskSystem.Dtos.Common;

namespace AutomatedTaskSystem.Dtos.Tasks;

public class GetStepAheadDto
{
    public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public BasicInfoDto Group { get; set; } = new BasicInfoDto { };
	public bool IsComplete { get; set; }
}
