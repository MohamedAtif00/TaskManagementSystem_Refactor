namespace AutomatedTaskSystem.DTO
{
    public static partial class Requests
    {
        public class StepDTO
        {
            public int TaskBankItem { get; set; }
            public int Duration { get; set; }
        }
    }

    public static partial class Responses
    {
        public class StepDTO
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public int Order { get; set; }
            public bool Reviewable { get; set; } = false;
            public bool TL { get; set; } = false;
            public IDName Group { get; set; } = new IDName { };
            public int Duration { get; set; }
        }
    }
}
