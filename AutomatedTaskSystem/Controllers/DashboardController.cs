using AutomatedTaskSystem.Dtos.Dashboard.GetMemberDashboard;
using AutomatedTaskSystem.Dtos.Dashboard.GetProjectManagerDashboard;
using AutomatedTaskSystem.Dtos.Dashboard.GetTeamLeaderDashboard;
using AutomatedTaskSystem.Services.DashboardService;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Controllers;

[Route("dashboards")]
[ApiController]
public class DashboardContoller : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardContoller(IDashboardService dashboardService) =>
        _dashboardService = dashboardService;

    [Authorize]
    [HttpGet("project-manager")]
    public async Task<ActionResult<ResponseService<GetProjectManagerDashboardDto>>> GetPMDB() =>
        await _dashboardService.GetProjectManagerDashboard();

    [Authorize]
    [HttpGet("team-leader")]
    public async Task<ActionResult<ResponseService<GetTeamLeaderDashboardDto>>> GetTLDB() =>
        await _dashboardService.GetTeamLeaderDashboard();

    [Authorize]
    [HttpGet("member")]
    public async Task<ActionResult<ResponseService<GetMemberDashboardDto>>> GetMemberDB() =>
        await _dashboardService.GetMemberDashboard();
}
