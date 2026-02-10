using AutomatedTaskSystem.Dtos.SprintDtos;
using AutomatedTaskSystem.Services.ResponseService;

namespace AutomatedTaskSystem.Services.Sprint
{
    public interface ISprintAnalyticsService
    {
        Task<ResponseService<SprintOverviewDto>> GetSprintOverviewAsync(int sprintId);
        Task<ResponseService<SprintLearningObjectivesProgressDto>> GetSprintLearningObjectivesProgressAsync(int sprintId);
        Task<ResponseService<SprintLearningObjectivesTableDto>> GetSprintLearningObjectivesTableAsync(int sprintId);
    }
}

