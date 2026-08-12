using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models.Enums;
using AutomatedTaskSystem.Services.AuthService;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.SessionTracking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Controllers;

[Route("auth")]
[ApiController]
public class AuthController : ControllerBase
{
	private readonly IAuthService _authService;
	private readonly IUserSessionService _sessionService;

	public AuthController(IAuthService authService, IUserSessionService sessionService)
	{
		_authService = authService;
		_sessionService = sessionService;
	}

	[Authorize]
	[HttpPost("about-me")]
	public async Task<ActionResult<ResponseService<Responses.AuthInfoDTO>>> AboutUser()
	=> await _authService.AboutUser();

	[HttpPost("logout")]
	public async Task<ActionResult<BaseResponseService>> Logout()
	=> await _authService.Logout(Request, Response);

	[HttpPost("login")]
	public async Task<ActionResult<ResponseService<string>>> Login(Requests.LoginDTO req)
	=> await _authService.Login(req.Code, Request, Response);

	[HttpPost("refresh-token")]
	public async Task<ActionResult<ResponseService<string>>> RefreshJWT()
	=> await _authService.RefreshToken(Request, Response);

	[HttpPost("session-expired")]
	public async Task<ActionResult<BaseResponseService>> SessionExpired()
	=> await _authService.EndSessionExpired(Request, Response);

	[Authorize]
	[HttpPost("force-logout/{userId:int}")]
	public async Task<ActionResult<BaseResponseService>> ForceLogout(int userId)
	=> await _authService.ForceLogout(userId);

	[Authorize]
	[HttpGet("sessions")]
	public async Task<ActionResult<ResponseService<UserSessionListDto>>> GetSessions(
		[FromQuery] int? userId,
		[FromQuery] DateTime? from,
		[FromQuery] DateTime? to,
		[FromQuery] SessionLogoutReason? reason,
		[FromQuery] int page = 1,
		[FromQuery] int pageSize = 25)
	=> await _sessionService.GetSessionsAsync(new UserSessionFilterDto
	{
		UserId = userId,
		From = from,
		To = to,
		Reason = reason,
		Page = page,
		PageSize = pageSize
	});

	[Authorize]
	[HttpGet("sessions/day")]
	public async Task<ActionResult<ResponseService<UserDayStorylineDto>>> GetDayStoryline(
		[FromQuery] int userId,
		[FromQuery] DateTime date)
	=> await _sessionService.GetDayStorylineAsync(userId, date);
}
