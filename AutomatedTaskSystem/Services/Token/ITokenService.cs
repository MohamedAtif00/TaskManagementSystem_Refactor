using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;

namespace AutomatedTaskSystem.Services.TokenService;

public interface ITokenService
{
	string CreateToken(User user);
	RefreshToken GenerateRefreshToken(User user);
	ResponseService<string> GetUserIdFromToken();
}
