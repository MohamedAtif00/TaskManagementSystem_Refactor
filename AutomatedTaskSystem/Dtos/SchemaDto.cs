namespace AutomatedTaskSystem.DTO
{
    public static partial class Responses
    {
        public class TaskBankDTO
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public bool TL { get; set; } = false;
            public IDName Type { get; set; } = new IDName { };
            public IDName Group { get; set; } = new IDName { };
            public int Duration { get; set; }
        }
        public class SchemaDTO
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public int Tasks { get; set; }

        }
        public class DetailedSchemaDTO
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            // public List<SchemaNodeDTO> Nodes { get; set; } = new List<SchemaNodeDTO> { };

        }
        public class SchemaNodeDTO
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public List<SchemaStepDTO> Steps { get; set; } = new List<SchemaStepDTO> { };
            public List<SchemaNodeNodeDTO> Previous { get; set; } = new List<SchemaNodeNodeDTO> { };
            public List<SchemaNodeNodeDTO> Next { get; set; } = new List<SchemaNodeNodeDTO> { };
            public List<SchemaNodeNodeDTO> Required { get; set; } = new List<SchemaNodeNodeDTO> { };
            public List<SchemaNodeNodeDTO> Requires { get; set; } = new List<SchemaNodeNodeDTO> { };
        }
        public class SchemaNodeNodeDTO
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
        }
        public class SchemaStepDTO
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public int Order { get; set; }
            public bool TL { get; set; } = false;
            public List<SchemaStepGroupDTO> Groups { get; set; } = new List<SchemaStepGroupDTO> { };
            public bool Reviewable { get; set; } = false;
        }
        public class SchemaStepGroupDTO
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
        }
    }
    public static partial class Requests
    {
        public class SchemaDTO
        {
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
        }
        public class TaskBankDTO
        {
            public string Name { get; set; } = "";
            public bool TL { get; set; } = false;
            public int Type { get; set; }
            public int Group { get; set; }
            public int Duration { get; set; }
        }
    }
}
