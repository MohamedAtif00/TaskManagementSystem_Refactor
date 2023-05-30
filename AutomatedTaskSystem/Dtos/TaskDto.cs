namespace AutomatedTaskSystem.DTO
{
    public static partial class Responses
    {
        public class TaskAssignmentDTO
        {
            public IDName? AssignedUser { get; set; } = null;
            public List<IDName> AssignableUsers { get; set; } = new List<IDName> { };
        }

        public class ITaskDTO
        {
            public Boolean Error { get; } = false;
            public Boolean Pause { get; set; } = false;
            public Boolean Attention { get; set; } = false;
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Status { get; set; } = "Backlog";
            public bool TL { get; set; } = false;
            public IDName Group { get; set; } = new IDName { };
            public IDName? User { get; set; } = new IDName { };
            public IDName LearningObjective { get; set; } = new IDName { };
            public IDName Schema { get; set; } = new IDName { };
            public bool IsReview { get; set; } = false;
            public bool Flagged { get; set; } = false;
            public string Lesson { get; set; } = "";
            public string Unit { get; set; } = "";
            public string Project { get; set; } = "";
            public string Tag { get; set; } = "";
            public string Environment { get; set; } = "";
            public string Template { get; set; } = "";
            public List<CommentDTO> Comments { get; set; } = new List<CommentDTO> { };
            public DateTime CreatedAt { get; set; }
            public DateTime? StartedAt { get; set; }
            public DateTime? DoneAt { get; set; }
            public string? From { get; set; }
            public int? Priority { get; set; }
        }

        public class TaskDTO
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Status { get; set; } = "Backlog";
            public bool TL { get; set; } = false;
            public string? Group { get; set; }
            public int? GroupId { get; set; }
            public string? User { get; set; }
            public int? UserId { get; set; }
            public string LearningObjective { get; set; } = "";
            public int LearningObjectiveId { get; set; }
            public bool IsReview { get; set; } = false;
            public bool Flagged { get; set; } = false;
            public bool Attention { get; set; } = false;
            public string Lesson { get; set; } = "";
            public string Unit { get; set; } = "";
            public string Project { get; set; } = "";
            public int ProjectId { get; set; }
            public string ProjectDescription { get; set; } = "";
            public string Tag { get; set; } = "";
            public string Environment { get; set; } = "";
            public string Template { get; set; } = "";
            public List<CommentDTO> Comments { get; set; } = new List<CommentDTO> { };
            public bool IsRollback { get; set; } = false;
            public int RollbackCount { get; set; }
            public string? From { get; set; }
            public int? Priority { get; set; } = null;
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
            public int? Priority { get; set; } = null;
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
            public int TaskId { get; set; }
            public int StepId { get; set; }
        }
    }
}
