namespace AutomatedTaskSystem.Models.Enums.TaskActivityType;

public enum TaskActivityTypeEnum
{
    None,
    Created,
    Status_ToDo,
    Status_Doing,
    Status_Done,
    Status_Rollback,
    Pause,
    Resume,
    Flag,
    Unflag,
    Assign,
    Comment,
    Rollback,
    PriorityChange,
	Skip,
	ProcessChange
}
