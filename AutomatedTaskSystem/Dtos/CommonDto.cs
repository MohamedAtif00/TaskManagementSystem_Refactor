namespace AutomatedTaskSystem.DTO
{
    public static partial class Responses
    {
        public class BadRequestsDTO
        {
            public bool Error { get; } = true;
            public string Info { get; set; } = "";

            public BadRequestsDTO(string info)
            {
                Info = info;
            }
        }
        public class SuccessDTO
        {
            public bool Error { get; } = false;
            public String Info { get; set; } = "";

            public SuccessDTO(string info)
            {
                Info = info;
            }
        }
    }
}
