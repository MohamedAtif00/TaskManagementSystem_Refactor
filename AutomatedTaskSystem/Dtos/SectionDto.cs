namespace AutomatedTaskSystem.DTO
{
	public static partial class Requests
	{
		public class SectionDTO
		{
			public string Name { get; set; } = "";
			public int HeadId { get; set; }
			public List<int> Groups { get; set; } = new List<int> { };
		}
	}

	public static partial class Responses
	{
		public class SectionDTO
		{
			public int Id { get; set; }
			public string Name { get; set; } = "";
			public IDName Head { get; set; } = new IDName { };
			public List<IDName> Groups { get; set; } =new List<IDName> { };
		}
		public class IDName
		{
			public int Id { get; set; }
			public string Name { get; set; } = "";
		}
	}
}