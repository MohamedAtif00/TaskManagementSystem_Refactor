using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Dtos.Tasks;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.TaskService;

public interface ITaskService
{
    Task<Models.Task> CreateTask(Step step, LearningObjective lo);
    Task<Models.Task> CreateTask(Step step, LearningObjective lo, Models.Task? from);
    Task<Models.Task> CreateTask(TaskBank taskBank, LearningObjective lo);
    Task<ActionResult<ResponseService<GetTaskDetailsDto>>> CreateTask(
        int taskBankId,
        int loId,
        int userId
    );
    Task<ActionResult<ResponseService<Responses.ITaskDTO>>> UpdateTaskPriority(
        int TaskId,
        int? Priority
    );
    Task<ActionResult<ResponseService<GetTaskDetailsDto>>> GetTaskDetails(int id);
    Task<ActionResult<ResponseService<GetTaskDetailsDto>>> TogglePause(int id);
    Task<ActionResult<ResponseService<GetTaskDetailsDto>>> ToggleFlag(int id);
    Task<ActionResult<ResponseService<GetTaskAssignmentDto>>> GetTaskAssignment(int id);
    Task<ActionResult<BaseResponseService>> AssignUser(int id, int uid);
    Task<ActionResult<ResponseService<List<GetTaskCardDto>>>> GetProjectTask(int pid);
    Task<ActionResult<ResponseService<GetTaskDetailsDto>>> RollbackTask(int taskId, int stepId);
    Task<ActionResult<ResponseService<GetTaskDetailsDto>>> ProceedTask(int taskId);
    Task<ActionResult<ResponseService<GetCreatableTasks>>> CreatableTasks(int projectId);
    Task<bool> CreateNext(Models.Task task);
    Task<bool> CreateNext(int taskId);
    Task<bool> CreateNextNode(int nodeId, int loId);
}
