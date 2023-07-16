using AutomatedTaskSystem.Models.SchemaTypesModel;

namespace AutomatedTaskSystem.Models;

public class Schema
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<Node> Nodes { get; set; } = new List<Node> { };
    public List<LearningObjective> LearningObjectives { get; set; } =
        new List<LearningObjective> { };
    public bool Archived { get; set; } = false;
    public SchemaType? Type { get; set; } = null;
    public int? TypeId { get; set; }
}
