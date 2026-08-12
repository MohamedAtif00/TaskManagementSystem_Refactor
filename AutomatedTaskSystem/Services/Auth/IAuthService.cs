using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.AuthService;
public interface IAuthService
{
	Task<ActionResult<ResponseService<string>>> Login(string Code, HttpRequest request, HttpResponse response);
	Task<ActionResult<BaseResponseService>> Logout(HttpRequest request, HttpResponse response);
	Task<ActionResult<ResponseService<string>>> RefreshToken(HttpRequest request, HttpResponse response);
	Task<ActionResult<ResponseService<Responses.AuthInfoDTO>>> AboutUser();
	Task<ActionResult<BaseResponseService>> ForceLogout(int userId);
	Task<ActionResult<BaseResponseService>> EndSessionExpired(HttpRequest request, HttpResponse response);
	Task<User?> GetAuthedUser();
}
