using AutomatedTaskSystem.Dtos.Common;

namespace AutomatedTaskSystem.Dtos.Tasks;

public class GetRollbackDto
{
    public int Id { get; set; }
    public BasicInfoDto Task { get; set; } = new BasicInfoDto { };
    public string? Clarification { get; set; }
    public List<RollbackAttachmentDto> Attachments { get; set; } =
        new List<RollbackAttachmentDto> { };
}
