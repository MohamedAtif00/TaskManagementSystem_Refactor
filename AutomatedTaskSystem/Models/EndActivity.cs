namespace AutomatedTaskSystem.Models
{
    public class EndActivity
    {
        public int id { get; set; }
        public Task Task { get; set; } = new Task { };
        public int TaskId { get; set; }
        public User User { get; set; } = new User { };
        public int UserId { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime? EndDate { get; set; } = DateTime.Now;
        public EndActivityType? EndActivityType { get; set; } = new EndActivityType { };
        public int? EndActivityTypeId { get; set; }
    }
}
