namespace AutomatedTaskSystem.DTO
{
	public static partial class Requests
	{
	}

	public static partial class Responses
	{
		public class ReportDTO
		{
			public int Id { get; set; }
			public string Name { get; set; } = "";
			public string Description { get; set; } = "";
			public List<Unit> Units { get; set; } = new List<Unit> {};
		}

		public class Unit {
			public int Id { get; set; }
			public string Name { get; set; } = "";
			public List<Lesson> Lessons { get; set; } = new List<Lesson> {};
		}

		public class Lesson {
			public int Id { get; set; }
			public string Name { get; set; } = "";
			public List<LearningObjective> LearningObjectives { get; set; } = new List<LearningObjective>{};
		}

		public class LearningObjective {
			public int Id { get; set; }
			public string Name { get; set; } = "";
			public string Tag { get; set; } = "";
			public string Environment { get; set; } = "";
			public string Template { get; set; } = "";
			public IDName Schema { get; set; } = new IDName{};
			public List<Task> Tasks { get; set; } = new List<Task>{};
		}

		public class Task {
			public int Id { get; set; }
			public string Name { get; set; } = "";
			public string Status { get; set; } = "";
			public int StatusId { get; set; }
		}
	}
}
