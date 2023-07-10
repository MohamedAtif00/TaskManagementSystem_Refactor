using AutomatedTaskSystem.Dtos.Dashboard.GetProjectManagerDashboard;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.DashboardService;

public interface IDashboardService
{
    Task<ActionResult<ResponseService<GetProjectManagerDashboardDto>>> GetProjectManagerDashboard();
}
