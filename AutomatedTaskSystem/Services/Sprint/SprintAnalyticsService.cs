using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Dtos.SprintDtos;
using AutomatedTaskSystem.Models.Enums.TaskStatus;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.EntityFrameworkCore;

namespace AutomatedTaskSystem.Services.Sprint
{
    public class SprintAnalyticsService : ISprintAnalyticsService
    {
        private readonly DataContext _dataContext;

        public SprintAnalyticsService(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        /// <summary>
        /// Calculate the date range based on the time period filter
        /// </summary>
        /// <param name="timePeriod">The time period filter</param>
        /// <returns>A tuple containing the start date and end date for filtering</returns>
        private static (DateTime? startDate, DateTime? endDate) GetDateRangeFromTimePeriod(TimePeriodFilter? timePeriod)
        {
            if (timePeriod == null || timePeriod == TimePeriodFilter.AllTime)
            {
                return (null, null); // No date filtering
            }

            var now = DateTime.Now;
            var today = now.Date;

            return timePeriod switch
            {
                TimePeriodFilter.Today => (today, today.AddDays(1).AddTicks(-1)),
                TimePeriodFilter.LastWeek => (today.AddDays(-7), now),
                TimePeriodFilter.LastMonth => (today.AddMonths(-1), now),
                _ => (null, null)
            };
        }

        public async Task<ResponseService<SprintOverviewDto>> GetSprintOverviewAsync(int sprintId, TimePeriodFilter? timePeriod = null)
        {
            var response = new ResponseService<SprintOverviewDto>();

            try
            {
                // Calculate date range based on time period
                var (startDate, endDate) = GetDateRangeFromTimePeriod(timePeriod);

                // Verify sprint exists
                var sprint = await _dataContext.Sprints
                    .Include(s => s.SprintLearningObjectives.Where(slo => slo.LearningObjective != null && !slo.LearningObjective.Archived))
                        .ThenInclude(slo => slo.LearningObjective)
                            .ThenInclude(lo => lo.Tasks)
                                .ThenInclude(t => t.Group)
                    .FirstOrDefaultAsync(s => s.Id == sprintId);

                if (sprint == null)
                {
                    response.Error = true;
                    response.Message = "Sprint not found.";
                    return response;
                }

                // Get all tasks for this sprint (excluding archived tasks)
                var allTasksQuery = sprint.SprintLearningObjectives
                    .SelectMany(slo => slo.LearningObjective?.Tasks ?? new List<Models.Task>())
                    .Where(t => !t.Archived);

                // Apply date filtering if specified
                if (startDate.HasValue && endDate.HasValue)
                {
                    allTasksQuery = allTasksQuery.Where(t => t.CreatedAt >= startDate.Value && t.CreatedAt <= endDate.Value);
                }

                var allTasks = allTasksQuery.ToList();

                // Calculate task summary
                var taskSummary = new TaskSummaryDto
                {
                    Active = allTasks.Count(t => t.Status == TaskStatusEnum.ToDo || t.Status == TaskStatusEnum.Doing),
                    Completed = allTasks.Count(t => t.Status == TaskStatusEnum.Done),
                    Rollback = allTasks.Count(t => t.IsRollback),
                    Flagged = allTasks.Count(t => t.Flagged),
                    NotStarted = allTasks.Count(t => t.Status == TaskStatusEnum.Backlog),
                    Total = allTasks.Count
                };

                // Calculate tag distribution (group by Group)
                var tagData = allTasks
                    .Where(t => t.Group != null)
                    .GroupBy(t => new { t.GroupId, t.Group.Name, t.Group.ColorCode })
                    .Select(g => new TagDataDto
                    {
                        Label = g.Key.Name,
                        Value = g.Count(),
                        Color = string.IsNullOrEmpty(g.Key.ColorCode) ? "#6b7280" : g.Key.ColorCode,
                        IsFilled = false
                    })
                    .Where(tag => tag.Value > 0) // Only include groups with tasks
                    .ToList();

                // Calculate learning objectives summary with mutually exclusive categorization
                // All LOs must be categorized: Completed + NotStarted + InProcess = Total
                // Apply date filtering to tasks when calculating LO status

                // Helper function to filter tasks by date range
                Func<IEnumerable<Models.Task>, IEnumerable<Models.Task>> filterTasksByDate = (tasks) =>
                {
                    var filtered = tasks.Where(t => !t.Archived);
                    if (startDate.HasValue && endDate.HasValue)
                    {
                        filtered = filtered.Where(t => t.CreatedAt >= startDate.Value && t.CreatedAt <= endDate.Value);
                    }
                    return filtered;
                };

                var totalLOs = sprint.SprintLearningObjectives.Count;

                // Completed: All filtered non-archived tasks are Done (must have at least one filtered task)
                var completedLOs = sprint.SprintLearningObjectives
                    .Count(slo => slo.LearningObjective != null &&
                                  slo.LearningObjective.Tasks != null &&
                                  filterTasksByDate(slo.LearningObjective.Tasks).Any() &&
                                  filterTasksByDate(slo.LearningObjective.Tasks).All(t => t.Status == TaskStatusEnum.Done));

                // Not Started: All filtered non-archived tasks are Backlog (must have at least one filtered task)
                var notStartedLOs = sprint.SprintLearningObjectives
                    .Count(slo => slo.LearningObjective != null &&
                                  slo.LearningObjective.Tasks != null &&
                                  filterTasksByDate(slo.LearningObjective.Tasks).Any() &&
                                  filterTasksByDate(slo.LearningObjective.Tasks).All(t => t.Status == TaskStatusEnum.Backlog));

                // In Process: Everything else (LOs with no filtered tasks, mixed statuses, or any ToDo/Doing tasks)
                // This ensures completedLOs + notStartedLOs + inProcessLOs = totalLOs
                var inProcessLOs = totalLOs - completedLOs - notStartedLOs;

                var loSummary = new LearningObjectiveSummaryDto
                {
                    Completed = completedLOs,
                    NotStarted = notStartedLOs,
                    InProcess = inProcessLOs,
                    Total = totalLOs
                };

                response.Data = new SprintOverviewDto
                {
                    Tags = tagData,
                    TaskSummary = taskSummary,
                    LoSummary = loSummary,
                    SprintSummary = taskSummary // Same as task summary for now
                };

                response.Message = "Sprint overview retrieved successfully.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSprintOverviewAsync: {ex.Message}");
                Console.WriteLine(ex.StackTrace);

                response.Error = true;
                response.Message = $"An unexpected error occurred: {ex.Message}";
            }

            return response;
        }

        public async Task<ResponseService<SprintLearningObjectivesProgressDto>> GetSprintLearningObjectivesProgressAsync(int sprintId)
        {
            var response = new ResponseService<SprintLearningObjectivesProgressDto>();

            try
            {
                // Verify sprint exists
                var sprint = await _dataContext.Sprints
                    .Include(s => s.SprintLearningObjectives)
                        .ThenInclude(slo => slo.LearningObjective)
                            .ThenInclude(lo => lo.Tasks)
                    .FirstOrDefaultAsync(s => s.Id == sprintId);

                if (sprint == null)
                {
                    response.Error = true;
                    response.Message = "Sprint not found.";
                    return response;
                }

                // Calculate progress for each learning objective
                var progressData = sprint.SprintLearningObjectives
                    .Where(slo => slo.LearningObjective != null)
                    .Select(slo =>
                    {
                        var tasks = slo.LearningObjective!.Tasks ?? new List<Models.Task>();
                        var totalTasks = tasks.Count;
                        var completedTasks = tasks.Count(t => t.Status == TaskStatusEnum.Done);

                        // Calculate completion percentage
                        var completionPercentage = totalTasks > 0
                            ? (int)Math.Round((double)completedTasks / totalTasks * 100)
                            : 0;

                        // Determine status based on completion percentage
                        string status;
                        if (completionPercentage >= 75)
                            status = "On Track";
                        else if (completionPercentage >= 50)
                            status = "At Risk";
                        else
                            status = "Delayed";

                        return new LearningObjectiveProgressDto
                        {
                            Name = slo.LearningObjective.Name,
                            Value = completionPercentage,
                            Status = status
                        };
                    })
                    .Where(lo => lo.Value > 0 || sprint.SprintLearningObjectives.Count <= 10) // Include all if <= 10 LOs
                    .ToList();

                response.Data = new SprintLearningObjectivesProgressDto
                {
                    Data = progressData
                };

                response.Message = "Learning objectives progress retrieved successfully.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSprintLearningObjectivesProgressAsync: {ex.Message}");
                Console.WriteLine(ex.StackTrace);

                response.Error = true;
                response.Message = $"An unexpected error occurred: {ex.Message}";
            }

            return response;
        }

        public async Task<ResponseService<SprintLearningObjectivesTableDto>> GetSprintLearningObjectivesTableAsync(int sprintId)
        {
            var response = new ResponseService<SprintLearningObjectivesTableDto>();

            try
            {
                // Verify sprint exists and include all necessary relationships
                var sprint = await _dataContext.Sprints
                    .Include(s => s.SprintLearningObjectives)
                        .ThenInclude(slo => slo.LearningObjective)
                            .ThenInclude(lo => lo.Lesson)
                                .ThenInclude(l => l.Unit)
                    .Include(s => s.SprintLearningObjectives)
                        .ThenInclude(slo => slo.LearningObjective)
                            .ThenInclude(lo => lo.Tasks.Where(t => !t.Archived))
                                .ThenInclude(t => t.Group)
                    .FirstOrDefaultAsync(s => s.Id == sprintId);

                if (sprint == null)
                {
                    response.Error = true;
                    response.Message = "Sprint not found.";
                    return response;
                }

                // Build table data for each learning objective
                var tableData = sprint.SprintLearningObjectives
                    .Where(slo => slo.LearningObjective != null && !slo.LearningObjective.Archived)
                    .Select(slo =>
                    {
                        var lo = slo.LearningObjective!;
                        var tasks = lo.Tasks?.Where(t => !t.Archived).ToList() ?? new List<Models.Task>();
                        var totalTasks = tasks.Count;
                        var completedTasks = tasks.Count(t => t.Status == TaskStatusEnum.Done);
                        var activeTasks = tasks.Count(t => t.Status == TaskStatusEnum.ToDo || t.Status == TaskStatusEnum.Doing);

                        // Calculate completion percentage (progress)
                        var progress = totalTasks > 0
                            ? (int)Math.Round((double)completedTasks / totalTasks * 100)
                            : 0;

                        // Determine status based on completion percentage
                        string status;
                        if (progress >= 75)
                            status = "On Track";
                        else if (progress >= 50)
                            status = "At Risk";
                        else
                            status = "Delayed";

                        // Get current phases (unique groups of active tasks)
                        var currentPhases = tasks
                            .Where(t => t.Status == TaskStatusEnum.ToDo || t.Status == TaskStatusEnum.Doing || t.Status == TaskStatusEnum.Backlog)
                            .Where(t => t.Group != null)
                            .Select(t => new CurrentPhaseDto
                            {
                                GroupName = t.Group!.Name,
                                ColorCode = string.IsNullOrEmpty(t.Group.ColorCode) ? "#6b7280" : t.Group.ColorCode
                            })
                            .DistinctBy(p => p.GroupName)
                            .ToList();

                        // Get subject from lesson's unit name
                        var subject = (lo.Name ?? "") switch
                        {
                            var s when s.Contains("mth", StringComparison.OrdinalIgnoreCase) => "math",
                            var s when s.Contains("sci", StringComparison.OrdinalIgnoreCase) => "science",
                            var s when s.Contains("eng", StringComparison.OrdinalIgnoreCase) => "english",
                            var s when s.Contains("ara", StringComparison.OrdinalIgnoreCase) => "arabic",
                            var s when s.Contains("soc", StringComparison.OrdinalIgnoreCase) => "social study",
                            var s when s.Contains("mul", StringComparison.OrdinalIgnoreCase) => "multimedia",
                            var s when s.Contains("rel", StringComparison.OrdinalIgnoreCase) => "religion",
                            _ => "unknown" // The default case
                        };

                        // Format start date
                        var startDate = lo.StartedAt?.ToString("d/M/yyyy") ?? "";

                        return new LearningObjectiveTableRowDto
                        {
                            Id = lo.Id,
                            Name = lo.Name,
                            Subject = subject,
                            StartDate = startDate,
                            ActiveTasks = activeTasks,
                            CurrentPhases = currentPhases,
                            Status = status,
                            Progress = progress
                        };
                    })
                    .ToList();

                response.Data = new SprintLearningObjectivesTableDto
                {
                    Data = tableData
                };

                response.Message = "Learning objectives table data retrieved successfully.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSprintLearningObjectivesTableAsync: {ex.Message}");
                Console.WriteLine(ex.StackTrace);

                response.Error = true;
                response.Message = $"An unexpected error occurred: {ex.Message}";
            }

            return response;
        }
    }
}

