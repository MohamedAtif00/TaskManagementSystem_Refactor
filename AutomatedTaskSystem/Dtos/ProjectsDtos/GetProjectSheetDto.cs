using AutomatedTaskSystem.Dtos.Common;
using AutomatedTaskSystem.Dtos.Unit;

namespace AutomatedTaskSystem.Dtos.Projects;

public class GetProjectSheetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<GetUnitChipDto> Units { get; set; } = new List<GetUnitChipDto> { };
    public List<BasicInfoDto> WorkableTasks { get; set; } = new List<BasicInfoDto> { };
}
