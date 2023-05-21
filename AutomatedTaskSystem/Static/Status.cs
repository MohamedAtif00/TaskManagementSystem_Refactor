namespace AutomatedTaskSystem.Static
{
	public static class Statuses
	{
		public static int Backlog { get; set; } = 1;
		public static int ToDo { get; set; } = 2;
		public static int Doing { get; set; } = 3;
		public static int Done { get; set; } = 4;
		public static int Rollback { get; set; } = 5;
	}
}
