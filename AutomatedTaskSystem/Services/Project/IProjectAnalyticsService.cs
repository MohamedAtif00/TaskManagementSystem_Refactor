using AutomatedTaskSystem.Dtos.Projects;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.Sprint;

namespace AutomatedTaskSystem.Services.Project;

public interface IProjectAnalyticsService
{
    /// <param name="timePeriod">Optional: 1=Today, 2=Last Week, 3=Last Month, 4=All Time (default).</param>
    Task<ResponseService<ProjectOverviewAnalyticsDto>> GetProjectOverviewAsync(int projectId, TimePeriodFilter? timePeriod = null);

    /// <summary>
    /// Project-level Learning Objectives progress chart data (same shape as sprint).
    /// </summary>
    /// <param name="inProgressOnly">When true, only LOs that still have Backlog / To Do / Doing tasks (optionally for the selected group).</param>
    Task<ResponseService<ProjectLearningObjectivesProgressDto>> GetProjectLearningObjectivesProgressAsync(int projectId, TimePeriodFilter? timePeriod = null, int? groupId = null, bool inProgressOnly = false);

    /// <summary>
    /// Tasks still in Backlog, To Do, or Doing for the project, optionally filtered by group and time period.
    /// </summary>
    Task<ResponseService<ProjectInProgressTasksDto>> GetProjectInProgressTasksAsync(int projectId, TimePeriodFilter? timePeriod = null, int? groupId = null);

    /// <summary>
    /// Project-level Learning Objectives table data (same columns as sprint).
    /// </summary>
    Task<ResponseService<ProjectLearningObjectivesTableDto>> GetProjectLearningObjectivesTableAsync(int projectId);
}
