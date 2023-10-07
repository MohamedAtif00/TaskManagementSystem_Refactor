using AutomatedTaskSystem.Dtos.Common;

namespace AutomatedTaskSystem.Dtos.Tasks;

public class TaskCommentDto
{
    public int Id { get; set; }
    public string Content { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public BasicInfoDto User { get; set; } = new BasicInfoDto { };
	public bool IsEdited { get; set; } = false;
	public bool IsDeleted { get; set; } = false;
}
