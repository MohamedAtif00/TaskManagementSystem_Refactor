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
