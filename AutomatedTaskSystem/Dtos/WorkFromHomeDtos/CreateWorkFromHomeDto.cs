namespace AutomatedTaskSystem.Dtos.WorkFromHomeDtos
{
    public class CreateWorkFromHomeDto
    {
        public int UserId { get; set; }
        public string Date { get; set; } // The single date for the work-from-home request
        public string? NoteForManager { get; set; } // Optional note for the manager
    }
}
