using AutomatedTaskSystem.Dtos.Report;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.ReportService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Controllers
{
    [Route("subjects")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("{id}/reports")]
        public async Task<ActionResult<ResponseService<GetProjectReportDto>>> GetReports(int id) =>
            await _reportService.GetProjectReport(id);

        [HttpGet("reports")]
        public async Task<ActionResult<ResponseService<List<GetReportDto>>>> GetAllReports(
            [FromQuery(Name = "start")] DateTime? start,
            [FromQuery(Name = "end")] DateTime? end
        ) => await _reportService.GetAllProjectsReports(start: start, end: end);
    }
}
