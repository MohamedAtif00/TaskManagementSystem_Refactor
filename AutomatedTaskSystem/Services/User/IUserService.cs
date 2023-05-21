using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.UserService;

public interface IUserService
{
	Task<ActionResult<ResponseService<Responses.UserDTO>>> GetUserById(int Id);
	Task<ActionResult<ResponseService<Responses.UserAddedDTO>>> CreateUser(string Name, int GroupId, int RoleId);
	Task<ActionResult<ResponseService<List<Responses.UserDTO>>>> GetUsers();
}
