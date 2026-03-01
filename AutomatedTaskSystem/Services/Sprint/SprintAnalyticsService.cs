using System.Linq.Expressions;
using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Dtos.SprintDtos;
using AutomatedTaskSystem.Models;
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
                Expression<Func<SprintLearningObjective,bool>> expression = slo => slo.LearningObjective != null &&!slo.LearningObjective.Archived;
                // Calculate date range based on time period
                var (startDate, endDate) = GetDateRangeFromTimePeriod(timePeriod);

                // Verify sprint exists
                var sprint = await _dataContext.Sprints
                    .Include(s => s.SprintLearningObjectives.AsQueryable().Where(expression))
                        .ThenInclude(slo => slo.LearningObjective)
                            .ThenInclude(lo => lo.Tasks)
                                .ThenInclude(t => t.Group)
                    .Include(s => s.SprintLearningObjectives.AsQueryable().Where(expression))
                        .ThenInclude(slo => slo.LearningObjective)
                            .ThenInclude(lo => lo.Schema)
                                .ThenInclude(schema => schema.Nodes)
                                    .ThenInclude(node => node.Steps)
                                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == sprintId);

                if (sprint == null)
                {
                    response.Error = true;
                    response.Message = "Sprint not found.";
                    return response;
                }

                // Get ALL tasks for this sprint (excluding archived tasks) - UNFILTERED by time period
                // This is used for task summary pie charts which should always show complete data
                var allTasksUnfiltered = sprint.SprintLearningObjectives
                    .SelectMany(slo => slo.LearningObjective?.Tasks ?? new List<Models.Task>())
                    .Where(t => !t.Archived)
                    .ToList();

                // Get tasks filtered by time period for tags and learning objectives data
                var allTasksFiltered = allTasksUnfiltered.AsEnumerable();
                if (startDate.HasValue && endDate.HasValue)
                {
                    allTasksFiltered = allTasksFiltered.Where(t => t.CreatedAt >= startDate.Value && t.CreatedAt <= endDate.Value);
                }
                var allTasks = allTasksFiltered.ToList();


                // 1. Calculate Total Expected Tasks from Schema (The "Source of Truth")
                var totalExpectedTasks = sprint.SprintLearningObjectives
                    .Where(slo => slo.LearningObjective != null && !slo.LearningObjective.Archived)
                    .SelectMany(slo => slo.LearningObjective!.Schema?.Nodes?
                        .Where(node => !node.Archived)
                        .SelectMany(node => node.Steps ?? new List<Models.Step>())
                        .Where(step => !step.Archived) ?? new List<Models.Step>())
                    .Count();

                // 2. Get the physical tasks for task summary calculation (UNFILTERED - always show all tasks)
                var completedCount = allTasksUnfiltered.Count(t => t.Status == TaskStatusEnum.Done);
                var activeCount = allTasksUnfiltered.Count(t => t.Status == TaskStatusEnum.ToDo || t.Status == TaskStatusEnum.Doing);

                // 3. Calculate Not Started
                // Logic: Total Schema Steps - (Any task that has actually been moved out of Backlog/Created)
                // Or more simply: Total Expected - Completed - Active
                var notStartedCount = totalExpectedTasks - completedCount - activeCount;

                // Calculate task summary (UNFILTERED - always shows complete task status distribution)
                // Time period filter does NOT affect task summary pie charts
                var taskSummary = new TaskSummaryDto
                {
                    Active = allTasksUnfiltered.Count(t => t.Status == TaskStatusEnum.ToDo || t.Status == TaskStatusEnum.Doing),
                    Completed = allTasksUnfiltered.Count(t => t.Status == TaskStatusEnum.Done),
                    Rollback = allTasksUnfiltered.Count(t => t.IsRollback),
                    Flagged = allTasksUnfiltered.Count(t => t.Flagged),
                    NotStarted = Math.Max(0, notStartedCount),
                    Total = totalExpectedTasks
                };

                // Calculate tag distribution (FILTERED by time period)
                // Only count tasks with status Backlog, ToDo, or Doing (exclude Done and Rollback)
                var tagData = allTasks
                    .Where(t => t.Group != null &&
                           (t.Status == TaskStatusEnum.Backlog ||
                            t.Status == TaskStatusEnum.ToDo ||
                            t.Status == TaskStatusEnum.Doing))
                    .GroupBy(t => new { t.GroupId, t.Group.Name, t.Group.ColorCode })
                    .Select(g => new TagDataDto
                    {
                        GroupId = g.Key.GroupId,
                        Label = g.Key.Name,
                        Value = g.Count(),
                        Color = string.IsNullOrEmpty(g.Key.ColorCode) ? "#6b7280" : g.Key.ColorCode,
                        IsFilled = false
                    })
                    .Where(tag => tag.Value > 0) // Only include groups with tasks
                    .ToList();
                    
                // Calculate learning objectives summary with mutually exclusive categorization
                // All LOs must be categorized: Completed + NotStarted + InProcess = Total
                // NOTE: LO Summary is UNFILTERED - always shows complete LO status distribution regardless of time period
                // (Similar to Task Summary - time period filter does NOT affect this chart)

                var totalLOs = sprint.SprintLearningObjectives.Count;

                // Completed: All non-archived tasks are Done (must have at least one task)
                var completedLOs = sprint.SprintLearningObjectives
                    .Count(slo => slo.LearningObjective != null && !slo.LearningObjective.Archived &&
                                  slo.LearningObjective.Tasks != null &&
                                  slo.LearningObjective.Tasks.Where(t => !t.Archived).Any() &&
                                  slo.LearningObjective.Tasks.Where(t => !t.Archived).All(t => t.Status == TaskStatusEnum.Done));

                // Not Started: LO has no tasks created yet, OR all existing non-archived tasks have Backlog status
                // This considers the schema-defined expected tasks - if schema defines tasks but none are created, it's "Not Started"
                var notStartedLOs = sprint.SprintLearningObjectives
                    .Count(slo =>
                    {
                        if (slo.LearningObjective == null) return false;

                        var lo = slo.LearningObjective;
                        var nonArchivedTasks = (lo.Tasks ?? new List<Models.Task>()).Where(t => !t.Archived).ToList();

                        // Check if schema has expected tasks (non-archived steps in non-archived nodes)
                        var hasExpectedTasks = lo.Schema?.Nodes?
                            .Where(node => !node.Archived)
                            .SelectMany(node => node.Steps ?? new List<Models.Step>())
                            .Any(step => !step.Archived) ?? false;

                        // Not Started if:
                        // 1. No tasks created yet (but schema expects tasks), OR
                        // 2. All existing tasks are in Backlog status
                        if (nonArchivedTasks.Count == 0)
                        {
                            // No tasks created - count as Not Started if schema has expected tasks
                            return hasExpectedTasks;
                        }

                        // Has tasks - check if all are in Backlog status
                        return nonArchivedTasks.All(t => t.Status == TaskStatusEnum.Backlog);
                    });

                // In Process: Everything else (LOs with mixed statuses, or any ToDo/Doing tasks)
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

        public async Task<ResponseService<SprintLearningObjectivesProgressDto>> GetSprintLearningObjectivesProgressAsync(int sprintId, TimePeriodFilter? timePeriod = null,int? groupId = null)
        {
            var response = new ResponseService<SprintLearningObjectivesProgressDto>();

            try
            {
                // Calculate date range based on time period for filtering tasks
                var (startDate, endDate) = GetDateRangeFromTimePeriod(timePeriod);

                // Verify sprint exists and include Schema -> Nodes -> Steps -> TaskBank for calculating expected tasks and duration
                var query = _dataContext.Sprints
                        .Include(s => s.SprintLearningObjectives
                            .Where(slo => slo.LearningObjective != null && !slo.LearningObjective.Archived))
                            .ThenInclude(slo => slo.LearningObjective)
                                .ThenInclude(lo => lo.Tasks.Where(t => !t.Archived))
                                    .ThenInclude(t => t.Group)
                        .Include(s => s.SprintLearningObjectives)
                            .ThenInclude(slo => slo.LearningObjective)
                                .ThenInclude(lo => lo.Schema)
                                    .ThenInclude(schema => schema.Nodes)
                                        .ThenInclude(node => node.Steps)
                                            .ThenInclude(step => step.TaskBank)
                        .AsQueryable();

                var sprint = await query.AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == sprintId);

                if (sprint == null)
                {
                    response.Error = true;
                    response.Message = "Sprint not found.";
                    return response;
                }

                // Helper function to filter tasks by groupId
                Func<IEnumerable<Models.Task>, IEnumerable<Models.Task>> filterTasksByGroup = (tasks) =>
                {
                    if (groupId.HasValue)
                    {
                        return tasks.Where(t => t.GroupId == groupId.Value);
                    }
                    return tasks;
                };

                // Get all task IDs belonging to LOs in this sprint for TaskWorkTime calculation
                // Apply groupId filter to task IDs as well
                var allTaskIds = sprint.SprintLearningObjectives
                    .Where(slo => slo.LearningObjective != null)
                    .SelectMany(slo => slo.LearningObjective!.Tasks ?? new List<Models.Task>())
                    .Where(t => !t.Archived)
                    .Where(t => !groupId.HasValue || t.GroupId == groupId.Value)
                    .Select(t => t.Id)
                    .ToList();

                // Fetch TaskWorkTimes for all tasks and group by TaskId
                var taskWorkTimes = await _dataContext.TaskWorkTimes
                    .Where(twt => allTaskIds.Contains(twt.TaskId))
                    .ToListAsync();

                // Create a dictionary of task ID -> total actual duration in minutes from TaskWorkTime records (stored in milliseconds)
                var taskActualDurations = taskWorkTimes
                    .GroupBy(twt => twt.TaskId)
                    .ToDictionary(g => g.Key, g => g.Sum(twt => TimeSpan.FromMilliseconds(twt.Duration).TotalMinutes));

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

                // Filter learning objectives: only include LOs that have at least one task matching the groupId filter
                var filteredSprintLOs = sprint.SprintLearningObjectives
                    .Where(slo => slo.LearningObjective != null && !slo.LearningObjective.Archived)
                    .Where(slo => !groupId.HasValue ||
                        (slo.LearningObjective!.Tasks != null &&
                         slo.LearningObjective.Tasks.Any(t => !t.Archived && t.GroupId == groupId.Value)))
                    .ToList();

                // Calculate progress for each learning objective
                var progressQuery = filteredSprintLOs
                    .Select(slo =>
                    {
                        var lo = slo.LearningObjective!;
                        // Get all non-archived tasks, then filter by groupId if specified
                        var allTasks = lo.Tasks?.Where(t => !t.Archived).ToList() ?? new List<Models.Task>();
                        var groupFilteredTasks = filterTasksByGroup(allTasks).ToList();

                        // Apply time period filtering to the group-filtered tasks
                        var filteredTasks = filterTasksByDate(groupFilteredTasks).ToList();

                        // Get all steps from schema (excluding archived)
                        var steps = lo.Schema?.Nodes?
                            .Where(node => !node.Archived)
                            .SelectMany(node => node.Steps ?? new List<Models.Step>())
                            .Where(step => !step.Archived)
                            .ToList() ?? new List<Models.Step>();

                        // Calculate total expected tasks from schema steps count
                        var totalExpectedTasks = steps.Count;

                        // Calculate total expected duration from TaskBank.Duration for all schema steps
                        var totalExpectedDuration = steps
                            .Where(step => step.TaskBank != null)
                            .Sum(step => step.TaskBank.Duration);

                        // Calculate total actual time spent from TaskWorkTime.Duration for all tasks in this LO
                        var totalActualTimeSpent = filteredTasks
                            .Where(t => taskActualDurations.ContainsKey(t.Id))
                            .Sum(t => taskActualDurations[t.Id]);

                        // Count completed tasks (status = Done) from filtered tasks
                        var completedTasks = filteredTasks.Count(t => t.Status == TaskStatusEnum.Done);

                        // Calculate completion percentage based on expected tasks from schema
                        var completionPercentage = totalExpectedTasks > 0
                            ? Math.Min(100, (int)Math.Round((double)completedTasks / totalExpectedTasks * 100))
                            : 0;

                        // Determine status based on time-based delay detection first, then completion percentage
                        string status;

                        // Time-based delay detection: if total logged time exceeds expected duration, mark as Delayed
                        if (totalExpectedDuration > 0 && totalActualTimeSpent > totalExpectedDuration)
                        {
                            status = "Delayed";
                        }
                        // Otherwise, use completion percentage-based status
                        else if (completionPercentage >= 75)
                        {
                            status = "On Track";
                        }
                        else if (completionPercentage >= 50)
                        {
                            status = "At Risk";
                        }
                        else
                        {
                            status = "Delayed";
                        }

                        // Get current phases (unique groups of active tasks)
                        // Use groupFilteredTasks to be consistent with the group filtering
                        var currentPhases = groupFilteredTasks
                            .Where(t => t.Status == TaskStatusEnum.Backlog || t.Status == TaskStatusEnum.ToDo || t.Status == TaskStatusEnum.Doing)
                            .Where(t => t.Group != null)
                            .Select(t => new CurrentPhaseDto
                            {
                                GroupName = t.Group!.Name,
                                ColorCode = string.IsNullOrEmpty(t.Group.ColorCode) ? "#6b7280" : t.Group.ColorCode
                            })
                            .DistinctBy(p => p.GroupName)
                            .ToList();

                        return new LearningObjectiveProgressDto
                        {
                            Id  = lo.Id,
                            Name = lo.Name,
                            Value = completionPercentage,
                            Status = status,
                            CurrentPhases = currentPhases
                        };
                    });

                // When no specific group is selected and there are many LOs, hide 0%-progress items.
                // When a group filter is applied, always return all matching LOs (including 0%),
                // so that clicking on a tag never results in an unexpected empty list.
                if (!groupId.HasValue && filteredSprintLOs.Count > 10)
                {
                    progressQuery = progressQuery.Where(lo => lo.Value > 0);
                }

                var progressData = progressQuery.ToList();

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
                // Include Schema -> Nodes -> Steps for calculating expected tasks from schema
                var sprint = await _dataContext.Sprints
                    .Include(s => s.SprintLearningObjectives)
                        .ThenInclude(slo => slo.LearningObjective)
                            .ThenInclude(lo => lo.Lesson)
                                .ThenInclude(l => l.Unit)
                    .Include(s => s.SprintLearningObjectives)
                        .ThenInclude(slo => slo.LearningObjective)
                            .ThenInclude(lo => lo.Tasks.Where(t => !t.Archived))
                                .ThenInclude(t => t.Group)
                    .Include(s => s.SprintLearningObjectives)
                        .ThenInclude(slo => slo.LearningObjective)
                            .ThenInclude(lo => lo.Schema)
                                .ThenInclude(schema => schema.Nodes)
                                    .ThenInclude(node => node.Steps)
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

                        // Calculate total expected tasks from Schema -> Nodes -> Steps (excluding archived)
                        // Progress is based on schema-defined expected tasks, not just existing tasks
                        var totalExpectedTasks = lo.Schema?.Nodes?
                            .Where(node => !node.Archived)
                            .SelectMany(node => node.Steps ?? new List<Models.Step>())
                            .Count(step => !step.Archived) ?? 0;

                        // Count completed tasks (status = Done)
                        var completedTasks = tasks.Count(t => t.Status == TaskStatusEnum.Done);
                        var activeTasks = tasks.Count(t => t.Status == TaskStatusEnum.ToDo || t.Status == TaskStatusEnum.Doing);

                        // Calculate completion percentage based on expected tasks from schema
                        // This reflects how much of the schema-defined workflow has been completed
                        var progress = totalExpectedTasks > 0
                            ? (int)Math.Round((double)completedTasks / totalExpectedTasks * 100)
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
                    SprintName = sprint.Name,
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

