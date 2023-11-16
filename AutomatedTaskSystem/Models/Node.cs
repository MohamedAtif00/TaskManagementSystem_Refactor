namespace AutomatedTaskSystem.Models;

public class Node
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public Schema Schema { get; set; } = new Schema { };
    public int SchemaId { get; set; }
    public List<Step> Steps { get; set; } = new List<Step> { };
    public int Order { get; set; }
    public List<Node> Next { get; set; } = new List<Node> { };
    public List<Node> Previous { get; set; } = new List<Node> { };
    public bool isStart { get; set; } = false;
    public bool Archived { get; set; } = false;
    public bool isEnd { get; set; } = false;
}
