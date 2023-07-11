namespace AutomatedTaskSystem.Models.SchemaTypesModel;

public class SchemaType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Schema> Schemas { get; set; } = new List<Schema> { };
}
