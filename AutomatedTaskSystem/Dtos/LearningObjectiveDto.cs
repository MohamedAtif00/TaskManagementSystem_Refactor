namespace AutomatedTaskSystem.DTO
{
    public static partial class Responses { }

    public static partial class Requests
    {
        public class EditLearningObjectiveDTO
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Tag { get; set; } = "";
            public string Environment { get; set; } = "";
            public string Template { get; set; } = "";
            public int SchemaId { get; set; }
			public List<int> Steps { get; set; } = new List<int>{};
			public List<int> Nods { get; set; } = new List<int>{};
        }
    }
}
