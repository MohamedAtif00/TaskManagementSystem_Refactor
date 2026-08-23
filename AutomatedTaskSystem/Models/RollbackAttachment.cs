namespace AutomatedTaskSystem.Models;

public class RollbackAttachment
{
    public int Id { get; set; }
    public Rollback Rollback { get; set; } = new Rollback { };
    public int RollbackId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
