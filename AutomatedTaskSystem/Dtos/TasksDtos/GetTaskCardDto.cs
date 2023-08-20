using AutomatedTaskSystem.Dtos.Common;

namespace AutomatedTaskSystem.Dtos.Tasks;

public class GetTaskCardDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Status { get; set; } = "";
    public bool TL { get; set; }
    public BasicInfoDto? User { get; set; }
    public BasicInfoDto LearningObjective { get; set; } = new BasicInfoDto { };
    public bool IsReview { get; set; }
    public bool Flagged { get; set; }
    public bool Paused { get; set; }
    public bool Attention { get; set; }
    public bool IsRollback { get; set; }
    public int RollbackCount { get; set; }
    public string From { get; set; } = "";
    public int? Priority { get; set; }
}
