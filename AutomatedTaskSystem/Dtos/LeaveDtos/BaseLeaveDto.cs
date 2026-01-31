namespace AutomatedTaskSystem.Dtos.LeaveDtos
{
    public class BaseLeaveDto
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string? Reason { get; set; } = null;
    }
}
