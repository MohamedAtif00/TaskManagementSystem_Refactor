using AutomatedTaskSystem.Dtos.Common;

namespace AutomatedTaskSystem.Dtos.Schema;

public class GetStepPointDto
{
    public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public BasicInfoDto Group { get; set; } = new BasicInfoDto { };
}
