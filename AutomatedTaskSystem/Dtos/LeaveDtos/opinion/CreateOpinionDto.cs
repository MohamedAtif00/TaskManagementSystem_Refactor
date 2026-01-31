namespace AutomatedTaskSystem.Dtos.LeaveDtos
{
    public class CreateOpinionDto
    {
        public int LeaveRequestId { get; set; }
        public int UserId { get; set; }
        public bool IsApproved { get; set; } = false;
        public string? Comment { get; set; } = string.Empty;
    }
}
