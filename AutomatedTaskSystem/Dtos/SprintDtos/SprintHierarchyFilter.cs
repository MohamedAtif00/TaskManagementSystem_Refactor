namespace AutomatedTaskSystem.Dtos.SprintDtos
{
    public class SprintHierarchyFilter
    {
        public string? YearName { get; set; }
        public string? ProjectName { get; set; }
        public string? TermName { get; set; }
        public string? SubjectGroupName { get; set; }

        public bool HasAnyFilter =>
            !string.IsNullOrWhiteSpace(YearName) ||
            !string.IsNullOrWhiteSpace(ProjectName) ||
            !string.IsNullOrWhiteSpace(TermName) ||
            !string.IsNullOrWhiteSpace(SubjectGroupName);
    }
}
