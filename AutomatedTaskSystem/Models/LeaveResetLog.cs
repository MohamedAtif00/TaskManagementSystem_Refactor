namespace AutomatedTaskSystem.Models
{
    /// <summary>
    /// Tracks that the annual leave reset job has run for a given year.
    /// </summary>
    public class LeaveResetLog
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public DateTime ExecutedAt { get; set; }
    }
}
