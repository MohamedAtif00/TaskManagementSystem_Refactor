namespace AutomatedTaskSystem.Models;

public class DailyReportNoteOverride
{
    public int Id { get; set; }
    public Task Task { get; set; } = new();
    public int TaskId { get; set; }
    public string Notes { get; set; } = "";
    public User UpdatedBy { get; set; } = new();
    public int UpdatedByUserId { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
