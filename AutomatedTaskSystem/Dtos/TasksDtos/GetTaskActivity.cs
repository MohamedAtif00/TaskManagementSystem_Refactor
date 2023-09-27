using AutomatedTaskSystem.Dtos.Common;
using AutomatedTaskSystem.Models.Enums.TaskActivityType;

namespace AutomatedTaskSystem.Dtos.Tasks;

public class GetTaskActivity
{
    public int Id { get; set; }
	public TaskActivityTypeEnum Type { get; set; } = TaskActivityTypeEnum.None;
    public BasicInfoDto? SecondaryTask { get; set; } = null;
    public BasicInfoDto? ActorOne { get; set; } = null;
    public BasicInfoDto? ActorTwo { get; set; } = null;
    public DateTime TimeStamp { get; set; } = DateTime.Now;
	public string? AdditionalInfo { get; set; } = string.Empty;
}
