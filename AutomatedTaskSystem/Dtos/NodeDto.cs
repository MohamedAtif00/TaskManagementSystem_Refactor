using AutomatedTaskSystem.Models.Enums.TaskPriority;

namespace AutomatedTaskSystem.DTO
{
    public static partial class Requests
    {
        public class NodeDTO
        {
            public string Name { get; set; } = "";
            public bool IsStart { get; set; }
            public List<int> Previous { get; set; } = new List<int> { };
            public List<int> Requires { get; set; } = new List<int> { };
        }

        public class NodePointsDTO
        {
            public bool isStart { get; set; }
            public bool isEnd { get; set; }
            public int NodeId { get; set; }
        }

        public class NodeIdDTO
        {
            public int NodeId { get; set; }
        }
    }

    public static partial class Responses
    {
        public class NodeDTO
        {
            public int Id { get; set; }
            public int Order { get; set; }
            public string Name { get; set; } = "";
            public List<NodeStepDTO> Steps { get; set; } = new List<NodeStepDTO> { };
            public List<IDName> Required { get; set; } = new List<IDName> { };
            public List<IDName> Requires { get; set; } = new List<IDName> { };
            public List<IDName> Next { get; set; } = new List<IDName> { };
            public List<IDName> Previous { get; set; } = new List<IDName> { };
            public bool isStart { get; set; } = false;
            public bool isEnd { get; set; } = false;
        }

        public class NodeStepDTO
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public int Order { get; set; }
            public bool TL { get; set; } = false;
            public IDName Group { get; set; } = new IDName { };
            public bool Reviewable { get; set; } = false;
            public int Duration { get; set; }
            public TaskPriorityEnum Priority { get; set; } = TaskPriorityEnum.None;
            public int TaskBankItemId { get; set; }
        }

        public class NodeWithStepsDTO
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public List<IDName> Steps { get; set; } = new List<IDName> { };
        }
    }
}
