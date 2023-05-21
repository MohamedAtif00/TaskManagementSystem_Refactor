namespace AutomatedTaskSystem.DTO
{
	public static partial class Requests
	{
		public class LoginDTO
		{
			public string Code { get; set; } = "";
		}
	}

	public static partial class Responses
	{
		public class AuthTokensDTO
		{
			public string AccessToken { get; set; } = "";
			public string RefreshToken { get; set; } = "";
		}
		public class AuthInfoDTO
		{
			public string Name { get; set; } = "";
			public int Role { get; set; } = 0;
			public int Id { get; set; } = 0;
			public string Group { get; set; } = "";
		}
	}
}
