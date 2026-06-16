using AutomatedTaskSystem.Dtos.Tasks;
using AutomatedTaskSystem.Models.Enums.TaskPriority;

namespace AutomatedTaskSystem.DTO;

public static partial class Responses
{
    public class TaskAssignmentDTO
    {
        public IDName? AssignedUser { get; set; } = null;
        public List<IDName> AssignableUsers { get; set; } = new List<IDName> { };
    }

    public class ITaskDTO
    {
        public bool Pause { get; set; }
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public IDName LearningObjective { get; set; } = new IDName { };
        public string Tag { get; set; } = "";
        public string Template { get; set; } = "";
        public string Environment { get; set; } = "";
        public IDName Schema { get; set; } = new IDName { };
        public bool IsReview { get; set; }
        public string Status { get; set; } = "";
        public bool Flagged { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? DoneAt { get; set; }
        public int? Priority { get; set; }
        public List<TaskCommentDto> Comments { get; set; } = new List<TaskCommentDto> { };
    }

    public class PreviousNodeDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public List<PreviousNodeStep> Steps { get; set; } = new List<PreviousNodeStep> { };
    }

    public class PreviousNodeStep
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int Order { get; set; }
    }
}

public static partial class Requests
{
    public class PriorityUpdateDto
    {
        public TaskPriorityEnum Priority { get; set; } = TaskPriorityEnum.None;
    }

    public class NewTaskDTO
    {
        public int UserId { get; set; }
        public int TaskBankItemId { get; set; }
        public int LearningObjectiveId { get; set; }
    }

    public class TaskDTO
    {
        public string Name { get; set; } = "";
        public bool TL { get; set; } = false;
        public int? GroupId { get; set; }
        public int? UserId { get; set; }
        public bool IsReview { get; set; } = false;
    }

    public class TaskUserAssignDTO
    {
        public int UserId { get; set; }
    }

    public class RollbackDTO
    {
        public int StepId { get; set; }
        public List<RollbackLogDto> Logs { get; set; } = new List<RollbackLogDto> { };
        public string? Clarification { get; set; }
    }

    public class FlagTaskDTO
    {
        public int TeamLeaderId { get; set; }
        public string Comment { get; set; } = "";
    }
}
