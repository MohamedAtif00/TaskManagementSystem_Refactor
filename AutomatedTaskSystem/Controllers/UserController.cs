using AutomatedTaskSystem.DTO;
using Microsoft.AspNetCore.Mvc;
using AutomatedTaskSystem.Services.UserService;
using AutomatedTaskSystem.Services.ResponseService;

namespace AutomatedTaskSystem.Controllers
{
	[Route("users")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly IUserService _userService;

		public UserController(IUserService userService)
		{
			_userService = userService;
		}
		// POST:
		// Create new user
		[HttpPost]
		public async Task<ActionResult<ResponseService<Responses.UserAddedDTO>>> CreateUser(Requests.UserDTO req)
		=> await _userService.CreateUser(req.Name, req.GroupId, req.RoleId);
		// GET:
		// Get all users
		[HttpGet]
		public async Task<ActionResult<ResponseService<List<Responses.UserDTO>>>> GetUsers()
		=> await _userService.GetUsers();
		// GET:
		// Get One User
		[HttpGet("{id}")]
		public async Task<ActionResult<ResponseService<Responses.UserDTO>>> GetUser(int id)
		=> await _userService.GetUserById(id);
	}
}
