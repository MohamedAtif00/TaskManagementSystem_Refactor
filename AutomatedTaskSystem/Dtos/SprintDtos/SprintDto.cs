using AutomatedTaskSystem.DTO;

namespace AutomatedTaskSystem.Dtos.SprintDtos
{
    public class SprintDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime  EndDate { get; set; }
        public bool IsArchived { get; set; }
        public int LoNumber { get; set; }
        public double CompletePercintag { get; set; }
        public List<Responses.IDName> learningObjects { get; set; }
    }
}
