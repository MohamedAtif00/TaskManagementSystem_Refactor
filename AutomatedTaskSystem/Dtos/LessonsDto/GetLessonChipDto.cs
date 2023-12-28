using AutomatedTaskSystem.Dtos.LearningObjective;

namespace AutomatedTaskSystem.Dtos.Lessons;

public class GetLessonChipDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<GetLearningObjectiveChipDto> Los { get; set; } = new List<GetLearningObjectiveChipDto> { };
}
