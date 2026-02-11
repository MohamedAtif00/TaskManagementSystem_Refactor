using AutomatedTaskSystem.Dtos.SprintDtos;
using AutomatedTaskSystem.Services.ResponseService;

namespace AutomatedTaskSystem.Services.Sprint
{
    /// <summary>
    /// Time period filter options for sprint analytics
    /// </summary>
    public enum TimePeriodFilter
    {
        Today = 1,
        LastWeek = 2,
        LastMonth = 3,
        AllTime = 4
    }

    public interface ISprintAnalyticsService
    {
        /// <summary>
        /// Get sprint overview analytics with optional time period filtering
        /// </summary>
        /// <param name="sprintId">The ID of the sprint</param>
        /// <param name="timePeriod">Optional time period filter (1=Today, 2=Last Week, 3=Last Month, 4=All Time). Defaults to All Time.</param>
        Task<ResponseService<SprintOverviewDto>> GetSprintOverviewAsync(int sprintId, TimePeriodFilter? timePeriod = null);
        Task<ResponseService<SprintLearningObjectivesProgressDto>> GetSprintLearningObjectivesProgressAsync(int sprintId);
        Task<ResponseService<SprintLearningObjectivesTableDto>> GetSprintLearningObjectivesTableAsync(int sprintId);
    }
}

