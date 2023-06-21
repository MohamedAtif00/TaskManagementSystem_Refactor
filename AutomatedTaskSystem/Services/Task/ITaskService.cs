using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Dtos.Tasks;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.TaskService;

public interface ITaskService
{
    Task<Models.Task> CreateTaskWithStep(Step step, LearningObjective lo);
    Task<Models.Task> CreateTaskWithStep(Step step, LearningObjective lo, Models.Task? from);
    Task<Models.Task> CreateTask(TaskBank taskBank, LearningObjective lo);
    Task<ActionResult<ResponseService<Responses.ITaskDTO>>> UpdateTaskPriority(
        int TaskId,
        int? Priority
    );
    Task<ActionResult<ResponseService<GetTaskDetailsDto>>> GetTaskDetails(int id);
}
