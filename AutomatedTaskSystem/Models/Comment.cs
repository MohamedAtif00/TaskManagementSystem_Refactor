namespace AutomatedTaskSystem.Models
{
	public class Comment
	{
		public int Id { get; set; }
		public string Content { get; set; } = "";
		public DateTime Timestamp { get; set; } = DateTime.Now;
		public User User { get; set; } = new User { };
		public int UserId { get; set; }
		public Task Task { get; set; } = new Task { };
		public int TaskId { get; set; }
		public bool Archived { get; set; } = false;
	}
}
