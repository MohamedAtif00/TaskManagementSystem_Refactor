using AutomatedTaskSystem.Dtos.Common;
using AutomatedTaskSystem.Dtos.Tasks;

namespace AutomatedTaskSystem.Dtos.LearningObjective;

public class GetLearningObjectiveChipDto
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Tag { get; set; } = string.Empty;
	public string Environment { get; set; } = string.Empty;
	public string Template { get; set; } = string.Empty;
	public BasicInfoDto Schema { get; set; } = new BasicInfoDto {};
	public List<GetTaskChipDto> Tasks { get; set; } = new List<GetTaskChipDto> {};
}
