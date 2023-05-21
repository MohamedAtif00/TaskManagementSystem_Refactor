using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.AuthService;
public interface IAuthService
{
	Task<ActionResult<ResponseService<string>>> Login(string Code, HttpResponse response);
	Task<ActionResult<BaseResponseService>> Logout(HttpRequest request, HttpResponse response);
	Task<ActionResult<ResponseService<string>>> RefreshToken(HttpRequest request, HttpResponse response);
	Task<ActionResult<ResponseService<Responses.AuthInfoDTO>>> AboutUser();
}