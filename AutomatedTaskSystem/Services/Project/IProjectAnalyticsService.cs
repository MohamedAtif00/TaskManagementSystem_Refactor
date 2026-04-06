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
    Task<ResponseService<ProjectLearningObjectivesProgressDto>> GetProjectLearningObjectivesProgressAsync(int projectId, TimePeriodFilter? timePeriod = null, int? groupId = null);

    /// <summary>
    /// Project-level Learning Objectives table data (same columns as sprint).
    /// </summary>
    Task<ResponseService<ProjectLearningObjectivesTableDto>> GetProjectLearningObjectivesTableAsync(int projectId);
}
