namespace AutomatedTaskSystem.DTO
{
	public static partial class Responses
	{
		public class GroupDTO
		{
			public int Id { get; set; }
			public string Name { get; set; } = "";
			public string ColorCode { get; set; } = "";
			public int Members { get; set; }
		}
	}

	public static partial class Requests
	{
		public class GroupDTO
		{
			public string Name { get; set; } = "";
			public string ColorCode { get; set; } = "";
		}
	}
}