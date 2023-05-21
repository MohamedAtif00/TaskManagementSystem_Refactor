namespace AutomatedTaskSystem.Models
{
    public class Assignment
    {
        public int Id { get; set; }
        public User? To { get; set; } = new User { };
        public int? ToId { get; set; }
        public User? By { get; set; } = new User { };
        public int? ById { get; set; }
        public Task Task { get; set; } = new Task { };
        public int TaskId { get; set; }
		public DateTime TimeStamp { get; set; } = DateTime.Now;
    }
}
