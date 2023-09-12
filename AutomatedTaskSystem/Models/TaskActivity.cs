using AutomatedTaskSystem.Models.Enums.TaskActivityType;

namespace AutomatedTaskSystem.Models;

public class TaskActivity
{
    public int Id { get; set; }
	public TaskActivityType Type { get; set; } = TaskActivityType.None;
    public Task Task { get; set; } = new Task { };
    public int TaskId { get; set; }
    public User? ActorOne { get; set; } = new User { };
    public int? ActorOneId { get; set; }
    public User? ActorTwo { get; set; } = new User { };
    public int? ActorTwoId { get; set; }
    public DateTime TimeStamp { get; set; } = DateTime.Now;
	public string? AdditionalInfo { get; set; } = string.Empty;
}

// Desc: Task was created.
// id: 1, type: 1, taskId: 1, actorOne: null, actorTwo: null, timeStamp: now, additionalInfo: null
//
// Desc: Etsh add to their To Do.
// id: 2, type: 2, taskId: 1, actorOne: 1, actorTwo: null, timeStamp: now + 1, additionalInfo: null
//
// Desc: Etsh started the task.
// id: 3, type: 3, taskId: 1, actorOne: 1, actorTwo: null, timeStamp: now + 2, additionalInfo: null
//
// Desc: Etsh flagged the task.
// id: 4, type: 5, taskId: 1, actorOne: 1, actorTwo: null, timeStamp: now + 3, additionalInfo: null
//
// Desc: Aya Tarek cleared the flag.
// id: 5, type: 6, taskId: 1, actorOne: 2, actorTwo: null, timeStamp: now + 4, additionalInfo: null
//
// Desc: Ahmed Mady set task's prority to High.
// id: 6, type: 10, taskId: 1, actorOne: 3, actorTwo: null, timeStamp: now + 5, additionalInfo: High
//
// Desc: Etsh started the task.
// id: 7, type: 3, taskId: 1, actorOne: 1, actorTwo: null, timeStamp: now + 7, additionalInfo: null
