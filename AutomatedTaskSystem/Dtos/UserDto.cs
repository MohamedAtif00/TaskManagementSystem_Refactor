namespace AutomatedTaskSystem.DTO
{
	public static partial class Responses
	{
		public class UserDTO
		{
			public int Id { get; set; }
			public string Name { get; set; } = "";
			public IDName Group { get; set; } = new IDName { };
			public IDName Role { get; set; } = new IDName { };
		}
		public class UserAddedDTO
		{
			public string Code { get; set; } = "";
			public UserDTO User { get; set; } = new UserDTO { };
		}
	}

	public static partial class Requests
	{
		public class UserDTO
		{
			public string Name { get; set; } = "";
			public int GroupId { get; set; }
			public int RoleId { get; set; }
		}
	}
}
