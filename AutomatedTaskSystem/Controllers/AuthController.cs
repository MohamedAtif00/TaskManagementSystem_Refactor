using AutomatedTaskSystem.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutomatedTaskSystem.Services.AuthService;
using AutomatedTaskSystem.Services.ResponseService;

namespace AutomatedTaskSystem.Controllers;

[Route("auth")]
[ApiController]
public class AuthController : ControllerBase
{
	private readonly IAuthService _authService;

	public AuthController(IAuthService authService)
	=> _authService = authService;

	[Authorize]
	[HttpPost("about-me")]
	public async Task<ActionResult<ResponseService<Responses.AuthInfoDTO>>> AboutUser()
	=> await _authService.AboutUser();
	[HttpPost("logout")]
	public async Task<ActionResult<BaseResponseService>> Logout()
	=> await _authService.Logout(Request, Response);
	[HttpPost("login")]
	public async Task<ActionResult<ResponseService<string>>> Login(Requests.LoginDTO req)
	=> await _authService.Login(req.Code, Response);
	[HttpPost("refresh-token")]
	public async Task<ActionResult<ResponseService<string>>> RefreshJWT()
	=> await _authService.RefreshToken(Request, Response);
}