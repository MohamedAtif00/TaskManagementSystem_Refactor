using AutomatedTaskSystem.DTO;
using Microsoft.AspNetCore.Mvc;
using AutomatedTaskSystem.Services.UserService;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.UserTask;
using AutomatedTaskSystem.Dtos.UserTask;

namespace AutomatedTaskSystem.Controllers
{
    [Route("users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserTaskService _userTaskService;

        public UserController(IUserService userService, IUserTaskService userTaskService)
        {
            _userService = userService;
            _userTaskService = userTaskService;
        }

        // POST:
        // Create new user
        [HttpPost]
        public async Task<ActionResult<ResponseService<Responses.UserAddedDTO>>> CreateUser(
            Requests.UserDTO req
        ) => await _userService.CreateUser(req.Name, req.GroupId, req.Role);

        // GET:
        // Get all users
        [HttpGet]
        public async Task<ActionResult<ResponseService<List<Responses.UserDTO>>>> GetUsers() =>
            await _userService.GetUsers();

        // GET:
        // Get One User
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseService<Responses.UserDTO>>> GetUser(int id) =>
            await _userService.GetUserById(id);

        // PATCH:
        // Edit User
        [HttpPatch("{id}")]
        public async Task<ActionResult<ResponseService<Responses.UserDTO>>> EditUser(
            int id,
            Requests.UserDTO req
        ) => await _userService.EditUser(id, req.Name, req.GroupId, req.Role);

        // DELETE:
        // Archive User
        [HttpDelete("{id}")]
        public async Task<ActionResult<BaseResponseService>> DeleteUser(int id) =>
            await _userService.ArchiveUser(id);

        // GET:
        // Get User Tasks
        [HttpGet("tasks")]
        public async Task<ActionResult<ResponseService<List<UserTaskDto>>>> GetUserTasks() =>
            await _userTaskService.GetAvailableUsersTasks();

        // GET:
        // Get User Tasks
        [HttpGet("{id}/tasks")]
        public async Task<ActionResult<ResponseService<UserTaskInfo>>> GetUserTasks(int id) =>
            await _userTaskService.GetUserTasks(id);
    }
}
