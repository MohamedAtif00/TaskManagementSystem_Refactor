using AutomatedTaskSystem.Models;

namespace AutomatedTaskSystem.Services.TaskService;

public interface ITaskService
{
    Task<Models.Task> CreateTaskWithStep(Step step, LearningObjective lo);
    Task<Models.Task> CreateTaskWithStep(Step step, LearningObjective lo, Models.Task? from);
    Task<Models.Task> CreateTask(TaskBank taskBank, LearningObjective lo);
}
