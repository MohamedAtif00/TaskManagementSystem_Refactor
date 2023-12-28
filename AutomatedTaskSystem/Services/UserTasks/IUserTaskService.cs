using AutomatedTaskSystem.Dtos.UserTask;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.UserTask;

public interface IUserTaskService
{
    Task<ActionResult<ResponseService<List<UserTaskDto>>>> GetAvailableUsersTasks();
    Task<ActionResult<ResponseService<UserTaskInfo>>> GetUserTasks(int id);
}
