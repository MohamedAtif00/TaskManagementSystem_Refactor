namespace AutomatedTaskSystem.Dtos.WorkFromHomeDtos
{
    public class CreateWorkFromHomeOpinionDto
    {
        public int WorkFromHomeId { get; set; } // Add this
        public bool IsApproved { get; set; }
        public string? Comment { get; set; }
    }
}
