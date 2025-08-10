using AutomatedTaskSystem.Models;

namespace AutomatedTaskSystem.Dtos.PermissionDtos
{
    public class CreateBulkPermissionOpinionDto
    {
        public int[] Ids { get; set; } = Array.Empty<int>();
        public PermissionStatusEnum Status { get; set; }
    }
}
