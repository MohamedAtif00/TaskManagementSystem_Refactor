using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;
using AutomatedTaskSystem.Dtos.Report;

namespace AutomatedTaskSystem.Services.ReportService;

public interface IReportService
{
    Task<ActionResult<ResponseService<GetProjectReportDto>>> GetProjectReport(int id);
    Task<ActionResult<ResponseService<List<GetReportDto>>>> GetAllProjectsReports();
}
