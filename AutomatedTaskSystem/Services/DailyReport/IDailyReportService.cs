using AutomatedTaskSystem.Dtos.DailyReport;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.DailyReport;

public interface IDailyReportService
{
    Task<ActionResult<ResponseService<GetDailyReportDashboardDto>>> GetDashboardAsync(DailyReportFilterDto filter);
    Task<ActionResult<ResponseService<GetDailyReportPagedDto>>> GetRowsAsync(DailyReportFilterDto filter);
    Task<ActionResult<ResponseService<GetDailyReportSummaryDto>>> GetSummaryAsync(DailyReportFilterDto filter);
    Task<ActionResult<ResponseService<GetDailyReportChartDto>>> GetChartsAsync(DailyReportFilterDto filter);
    Task<ActionResult<ResponseService<GetDailyReportLookupsDto>>> GetLookupsAsync();
    Task<ActionResult<ResponseService<string>>> UpdateNotesAsync(int taskId, UpdateDailyReportNotesDto dto);
}
