using AutomatedTaskSystem.Dtos.Dashboard.GetMemberDashboard;
using AutomatedTaskSystem.Dtos.Dashboard.GetProjectManagerDashboard;
using AutomatedTaskSystem.Dtos.Dashboard.GetSectionHeadDashboard;
using AutomatedTaskSystem.Dtos.Dashboard.GetTeamLeaderDashboard;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.DashboardService;

public interface IDashboardService
{
    Task<ActionResult<ResponseService<GetProjectManagerDashboardDto>>> GetProjectManagerDashboard();
    Task<ActionResult<ResponseService<GetTeamLeaderDashboardDto>>> GetTeamLeaderDashboard();
    Task<ActionResult<ResponseService<GetSectionHeadDashboardDto>>> GetSectionHeadDashboard();
    Task<ActionResult<ResponseService<GetMemberDashboardDto>>> GetMemberDashboard();
}
