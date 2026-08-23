using AutomatedTaskSystem.Models.Enums;

namespace AutomatedTaskSystem.Dtos.DailyReport;

public class GetDailyReportRowDto
{
    public int TaskId { get; set; }
    public string Date { get; set; } = "";
    public string Team { get; set; } = "";
    public string Semester { get; set; } = "";
    public string Subjects { get; set; } = "";
    public string Grade { get; set; } = "";
    public string TaskName { get; set; } = "";
    public string LoCode { get; set; } = "";
    public string LoType { get; set; } = "";
    public string AssignedTo { get; set; } = "";
    public string Status { get; set; } = "";
    public string ProblemType { get; set; } = "";
    public string Priority { get; set; } = "";
    public string Notes { get; set; } = "";
}
