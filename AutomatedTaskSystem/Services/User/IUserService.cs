using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models.Enums.UserRole;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.UserService;

public interface IUserService
{
    Task<ActionResult<ResponseService<Responses.UserDTO>>> GetUserById(int Id);
    Task<ActionResult<ResponseService<Responses.UserAddedDTO>>> CreateUser(
        Requests.UserDTO req
    );
    Task<ActionResult<ResponseService<Responses.UserDTO>>> EditUser(
        int id,
        Requests.UserDTO req
    );
    Task<ActionResult<ResponseService<List<Responses.UserDTO>>>> GetUsers();
    Task<ActionResult<BaseResponseService>> ArchiveUser(int id);
}
