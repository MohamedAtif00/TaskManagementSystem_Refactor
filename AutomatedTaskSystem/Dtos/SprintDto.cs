using AutomatedTaskSystem.Dtos.Report;

namespace AutomatedTaskSystem.DTO
{
    public static partial class Responses
    {
        public class SprintDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public string? StartDate { get; set; }
            public string? EndDate { get; set; }
            public bool IsArchived { get; set; }
        }


    }


    public static partial class Request
    {
        public class CreateSprint
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public List<Responses.IDName> Los { get; set; }
        }

        public class UpdateSprint
        {
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public string? StartDate { get; set; }
            public string? EndDate { get; set; }
            public List<int> Los { get; set; }
        }

        public class ResolveLosByName
        {
            public List<string> Names { get; set; } = new();
        }
    }
}