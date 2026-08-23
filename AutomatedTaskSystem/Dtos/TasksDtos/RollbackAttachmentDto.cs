namespace AutomatedTaskSystem.Dtos.Tasks;

public class RollbackAttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = "";
    public string ContentType { get; set; } = "";
    public long FileSize { get; set; }
}
