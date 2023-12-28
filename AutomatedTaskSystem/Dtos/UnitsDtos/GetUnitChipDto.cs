using AutomatedTaskSystem.Dtos.Lessons;

namespace AutomatedTaskSystem.Dtos.Unit;

public class GetUnitChipDto
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public List<GetLessonChipDto> Lessons { get; set; } = new List<GetLessonChipDto>{};
}
