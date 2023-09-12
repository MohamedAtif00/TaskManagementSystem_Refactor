namespace AutomatedTaskSystem.Models;

public class Activity
{
    public int id { get; set; }
    public Task Task { get; set; } = new Task { };
    public int TaskId { get; set; }
    public User User { get; set; } = new User { };
    public int UserId { get; set; }
    public DateTime TimeStamp { get; set; } = DateTime.Now;
    public ActivityType ActivityType { get; set; } = new ActivityType { };
    public int ActivityTypeId { get; set; }
}
