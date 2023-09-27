using AutomatedTaskSystem.Models.Enums.TaskActivityType;

namespace AutomatedTaskSystem.Models;

public class TaskActivity
{
	public int Id { get; set; }
	public TaskActivityTypeEnum Type { get; set; } = TaskActivityTypeEnum.None;
	public Task Task { get; set; } = new Task { };
	public int TaskId { get; set; }
	public Task? TaskSecondary { get; set; }
	public int? TaskSecondaryId { get; set; }
	public User? ActorOne { get; set; } = null;
	public int? ActorOneId { get; set; }
	public User? ActorTwo { get; set; } = null;
	public int? ActorTwoId { get; set; }
	public DateTime TimeStamp { get; set; } = DateTime.Now;
	public string? AdditionalInfo { get; set; } = null;
}
