using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Models;

namespace AutomatedTaskSystem.Services.TaskService;

public class TaskService : ITaskService
{
	private readonly DataContext _context;

	public TaskService(DataContext context)
	{
		_context = context;
	}

	public async Task<Models.Task> CreateTask(TaskBank taskBank, LearningObjective lo)
	{
		return await createTask(taskBank, lo);
	}

	public async Task<Models.Task> CreateTaskWithStep(Step step, LearningObjective lo)
	{
		var backLogStatus = await _context.Statuses.Where(s => s.Id == 1).FirstOrDefaultAsync();

		var todoStatus = await _context.Statuses.Where(s => s.Id == 2).FirstOrDefaultAsync();

		var newTask = await createTask(step: step, learningObjective: lo, null);

		return newTask;
	}

	public async Task<Models.Task> CreateTaskWithStep(
		Step step,
		LearningObjective lo,
		Models.Task? from
	)
	{
		var backLogStatus = await _context.Statuses.Where(s => s.Id == 1).FirstOrDefaultAsync();

		var todoStatus = await _context.Statuses.Where(s => s.Id == 2).FirstOrDefaultAsync();

		var newTask = await createTask(step: step, learningObjective: lo, from);

		return newTask;
	}

	private async Task<Models.Task> createTask(
		TaskBank taskBank,
		LearningObjective learningObjective
	)
	{
		var status = await _context.Statuses
			.Where(s => s.Id == (taskBank.TL ? 2 : 1))
			.FirstOrDefaultAsync();

		if (status is null)
			throw new Exception($"Unable to find status of id {(taskBank.TL ? 2 : 1)}");

		var newTask = new Models.Task
		{
			Step = null,
			StepId = null,
			LearningObjective = learningObjective,
			LearningObjectiveId = learningObjective.Id,
			TL = taskBank.TL,
			From = null,
			FromId = null,
			Name = taskBank.Name,
			User = null,
			UserId = null,
			Group = taskBank.Group,
			GroupId = taskBank.GroupId,
			Pause = false,
			Status = status,
			StatusId = status.Id,
			Flagged = false,
			Archived = false,
			Comments = new List<Comment> { },
			IsReview = taskBank.TypeId == 3,
			Attention = false,
			CreatedAt = DateTime.Now,
			RollbackCount = 0,
			IsRollback = false
		};

		_context.Tasks.Add(newTask);
		await _context.SaveChangesAsync();

		return newTask;
	}

	private async Task<Models.Task> createTask(
		Step step,
		LearningObjective learningObjective,
		Models.Task? from
	)
	{
		var status = await _context.Statuses
			.Where(s => s.Id == (step.TaskBank.TL ? 2 : 1))
			.FirstOrDefaultAsync();

		if (status is null)
			throw new Exception($"Unable to find status of id {(step.TaskBank.TL ? 2 : 1)}");

		var prevTasks =
			from == null
				? 0
				: (
					await _context.Tasks
						.Where(
							t =>
								t.StepId == step.Id && t.LearningObjectiveId == learningObjective.Id
						)
						.ToListAsync()
				).Count;

		var newTask = new Models.Task
		{
			Step = step,
			StepId = step.Id,
			LearningObjective = learningObjective,
			LearningObjectiveId = learningObjective.Id,
			TL = step.TaskBank.TL,
			From = from,
			FromId = from is null ? null : from.Id,
			Name = step.TaskBank.Name,
			User = null,
			UserId = null,
			Group = step.TaskBank.Group,
			GroupId = step.TaskBank.GroupId,
			Pause = false,
			Status = status,
			StatusId = status.Id,
			Flagged = false,
			Archived = false,
			Comments = new List<Comment> { },
			IsReview = step.TaskBank.TypeId == 3,
			Attention = false,
			CreatedAt = DateTime.Now,
			RollbackCount = prevTasks,
			IsRollback = from is null ? false : true
		};

		_context.Tasks.Add(newTask);
		await _context.SaveChangesAsync();

		return newTask;
	}
}
