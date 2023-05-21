namespace AutomatedTaskSystem.Models
{
	public class Node
	{
		public int Id { get; set; }
		public string Name { get; set; } = "";
		public Schema Schema { get; set; } = new Schema { };
		public int SchemaId { get; set; }
		public List<Step> Steps { get; set; } = new List<Step> { };
		// Where the Node is required
		public List<Node> Required { get; set; } = new List<Node> { };
		// What the Node requires
		public List<Node> Requires { get; set; } = new List<Node> { };
		public List<Node> Next { get; set; } = new List<Node> { };
		public List<Node> Previous { get; set; } = new List<Node> { };
		public bool isStart { get; set; } = false;
		public bool isEnd { get; set; } = false;
	}
}