namespace AutomatedTaskSystem.DTO
{
	public static partial class Responses
	{
		public class MiniTeamDTO
		{
			public int Id { get; set; }
			public string Name { get; set; } = "";
			public int Members { get; set; }
		}
		public class TeamDTO
		{
			public int Id { get; set; }
			public string Name { get; set; } = "";
			public List<TeamUser> Users { get; set; } = new List<TeamUser> { };
		}
		public class TeamUser
		{
			public int Id { get; set; }
			public string Name { get; set; } = "";
			public string Group { get; set; } = "";
		}
	}

	public static partial class Requests
	{
		public class TeamDTO
		{
			public string Name { get; set; } = "";
		}
		public class TeamAssignmentDTO
		{
			public List<int> UserIds { get; set; } = new List<int> { };
		}
	}
}