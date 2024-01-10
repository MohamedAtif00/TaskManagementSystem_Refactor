using AutomatedTaskSystem.Dtos.Projects;
using AutomatedTaskSystem.Dtos.Tasks;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums.TaskPriority;
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
    Task<ActionResult<ResponseService<GetTaskDetailsDto>>> UpdateTaskPriority(
        int TaskId,
        TaskPriorityEnum Priority
    );
    Task<ActionResult<ResponseService<GetTaskDetailsDto>>> GetTaskDetails(int id);
    Task<ActionResult<ResponseService<GetTaskDetailsDto>>> TogglePause(int id);
    Task<ActionResult<ResponseService<GetTaskDetailsDto>>> ToggleFlag(int id);
    Task<ActionResult<ResponseService<GetTaskAssignmentDto>>> GetTaskAssignment(int id);
    Task<ActionResult<BaseResponseService>> AssignUser(int id, int uid);
    Task<ActionResult<ResponseService<List<GetTaskCardDto>>>> GetProjectTask(int pid);
    Task<ActionResult<ResponseService<GetProjectSheetDto>>> GetProjectTaskChips(int pid);
    Task<ActionResult<ResponseService<GetTaskDetailsDto>>> RollbackTask(
        int taskId,
        int stepId,
        List<RollbackLogDto> logs,
        string? clarification
    );
    Task<ActionResult<ResponseService<GetTaskDetailsDto>>> ProceedTask(int taskId);
    Task<ActionResult<ResponseService<GetTaskDetailsDto>>> CompleteTask(int taskId);
    Task<ActionResult<ResponseService<GetCreatableTasksDto>>> CreatableTasks(int projectId);
    Task<ActionResult<ResponseService<GetTaskDetailsDto>>> SkipTask(int id);
    Task<ActionResult<ResponseService<List<GetNodeAheadDto>>>> GetSchemaSteps(int id);
    Task<ActionResult<ResponseService<GetTaskDetailsDto>>> JumpTask(
        int id,
        List<PutJumpedTaskDto> options
    );
    Task<ActionResult<ResponseService<TaskCommentDto>>> AddComment(int id, string comment);
    Task<bool> CreateNext(Models.Task task);
    Task<bool> CreateNext(int taskId);
    Task<bool> CreateNextNode(int nodeId, int loId);
    Task<ActionResult<ResponseService<TaskCommentDto>>> EditComment(
        int taskId,
        int commentId,
        string content
    );
    Task<ActionResult<ResponseService<TaskCommentDto>>> DeleteComment(int taskId, int commentId);
    Task<BaseResponseService> CreateProcess(List<int> options, int schemaId, int loId);
}
