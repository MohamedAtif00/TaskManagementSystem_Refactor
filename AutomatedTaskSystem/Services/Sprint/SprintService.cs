using System.Data;
using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Dtos.SprintDtos;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums.ProjectStatus;
using AutomatedTaskSystem.Models.Enums.TaskStatus;
using AutomatedTaskSystem.Services.ResponseService;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AutomatedTaskSystem.Services.Sprint
{
    public class SprintService : ISprintService
    {
        private readonly DataContext dataContext;
        private readonly IMemoryCache _cache;

        // Cache configuration
        private const int CacheExpirationMinutes = 2; // Cache expires after 2 minutes
        private const string AllSprintsCacheKeyPrefix = "AllSprints_";
        private const string AllSprintsCacheVersionKey = "AllSprints_CacheVersion";
        private const int ProgressQueryTimeoutSeconds = 20;

        public SprintService(DataContext dataContext, IMemoryCache cache)
        {
            this.dataContext = dataContext;
            _cache = cache;
        }

        /// <summary>
        /// Generates a cache key for all sprints based on archived filter and hierarchy filters
        /// </summary>
        private string GetAllSprintsCacheKey(bool? archived, SprintHierarchyFilter? hierarchyFilter)
        {
            var version = GetAllSprintsCacheVersion();
            var archivedKey = archived switch
            {
                null => "All",
                true => "Archived",
                false => "Active",
            };

            if (hierarchyFilter is null || !hierarchyFilter.HasAnyFilter)
            {
                return $"{AllSprintsCacheKeyPrefix}v{version}_{archivedKey}";
            }

            var filterKey = string.Join(
                "_",
                new[]
                {
                    hierarchyFilter.YearName,
                    hierarchyFilter.ProjectName,
                    hierarchyFilter.TermName,
                    hierarchyFilter.SubjectGroupName,
                }.Select(v => string.IsNullOrWhiteSpace(v) ? "-" : v.Trim())
            );

            return $"{AllSprintsCacheKeyPrefix}v{version}_{archivedKey}_{filterKey}";
        }

        private int GetAllSprintsCacheVersion()
        {
            if (_cache.TryGetValue(AllSprintsCacheVersionKey, out int version))
            {
                return version;
            }

            _cache.Set(AllSprintsCacheVersionKey, 0, new MemoryCacheEntryOptions
            {
                Priority = CacheItemPriority.NeverRemove
            });
            return 0;
        }

        private void InvalidateAllSprintsCache()
        {
            var next = GetAllSprintsCacheVersion() + 1;
            _cache.Set(AllSprintsCacheVersionKey, next, new MemoryCacheEntryOptions
            {
                Priority = CacheItemPriority.NeverRemove
            });
        }

        private const string SprintHierarchyJoinSql = @"
            INNER JOIN SprintLearningObjectives slo_filter ON slo_filter.SprintId = s.Id
            INNER JOIN LearningObjectives lo_filter ON slo_filter.LearningObjectiveId = lo_filter.Id AND lo_filter.Archived = 0
            INNER JOIN Lessons les_filter ON lo_filter.LessonId = les_filter.Id
            INNER JOIN Units u_filter ON les_filter.UnitId = u_filter.Id
            INNER JOIN Subjects sub_filter ON u_filter.SubjectId = sub_filter.Id
            INNER JOIN SubjectGroups sg_filter ON sub_filter.SubjectGroupId = sg_filter.Id
            INNER JOIN CurriculumTerms ct_filter ON sg_filter.TermId = ct_filter.Id
            INNER JOIN CurriculumProjects cp_filter ON ct_filter.ProjectId = cp_filter.Id
            INNER JOIN AcademicYears ay_filter ON cp_filter.YearId = ay_filter.Id";

        private static IEnumerable<string> BuildHierarchyWhereClauses(SprintHierarchyFilter? hierarchyFilter)
        {
            if (hierarchyFilter is null)
            {
                return Array.Empty<string>();
            }

            var clauses = new List<string>();
            if (!string.IsNullOrWhiteSpace(hierarchyFilter.YearName))
            {
                clauses.Add("LTRIM(RTRIM(ay_filter.Name)) = LTRIM(RTRIM(@YearName))");
            }
            // Project is applied in-memory against ProjectNames so the list matches the Project column.
            if (!string.IsNullOrWhiteSpace(hierarchyFilter.TermName))
            {
                clauses.Add("LTRIM(RTRIM(ct_filter.Name)) = LTRIM(RTRIM(@TermName))");
            }
            if (!string.IsNullOrWhiteSpace(hierarchyFilter.SubjectGroupName))
            {
                clauses.Add("LTRIM(RTRIM(sg_filter.Name)) = LTRIM(RTRIM(@SubjectGroupName))");
            }

            return clauses;
        }

        /// <summary>
        /// Resolves the distinct curriculum project names the given learning objectives belong to.
        /// </summary>
        private async Task<List<string>> GetProjectNamesForLearningObjectivesAsync(IEnumerable<int> learningObjectiveIds)
        {
            var ids = learningObjectiveIds.Distinct().ToList();
            if (ids.Count == 0)
            {
                return new List<string>();
            }

            var names = await dataContext.LearningObjectives
                .AsNoTracking()
                .Where(lo => ids.Contains(lo.Id))
                .Select(lo => lo.Lesson.Unit.Subject.SubjectGroup.Term.Project.Name)
                .Distinct()
                .ToListAsync();

            return names
                .Select(name => (name ?? "").Trim())
                .Where(name => name.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name)
                .ToList();
        }

        /// <summary>
        /// A sprint is scoped to a single curriculum project; returns an error message when the
        /// learning objectives span more than one, otherwise null.
        /// </summary>
        private async Task<string?> ValidateSingleProjectScopeAsync(IEnumerable<int> learningObjectiveIds)
        {
            var projectNames = await GetProjectNamesForLearningObjectivesAsync(learningObjectiveIds);
            if (projectNames.Count <= 1)
            {
                return null;
            }

            return "All learning objectives in a sprint must belong to the same project. "
                + $"The selected learning objectives span {projectNames.Count} projects: {string.Join(", ", projectNames)}.";
        }

        private static bool SprintMatchesProject(SprintDTO sprint, string? projectName)
        {
            if (string.IsNullOrWhiteSpace(projectName))
            {
                return true;
            }

            var needle = projectName.Trim();
            return sprint.ProjectNames.Any(name =>
                string.Equals(name?.Trim(), needle, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Optimized version using Dapper for improved performance with caching.
        /// Replaces EF Core with direct SQL queries to minimize database round trips.
        /// Results are cached for improved performance on subsequent calls.
        /// </summary>
        public async Task<ResponseService<List<SprintDTO>>> GetAllSprints(bool? archived = null, SprintHierarchyFilter? hierarchyFilter = null)
        {
            try
            {
                // Try to get from cache first
                var cacheKey = GetAllSprintsCacheKey(archived, hierarchyFilter);
                if (_cache.TryGetValue(cacheKey, out List<SprintDTO>? cachedData) && cachedData != null)
                {
                    return new ResponseService<List<SprintDTO>>() { Data = cachedData };
                }

                await using var connection = new SqlConnection(dataContext.Database.GetConnectionString());
                await connection.OpenAsync();

                var hierarchyWhereClauses = BuildHierarchyWhereClauses(hierarchyFilter).ToList();

                // Query 1: Get all sprints with basic info and LO count
                var sprintsSql = @"
                    SELECT DISTINCT
                        s.Id,
                        s.Name,
                        s.Description,
                        s.StartDate,
                        s.EndDate,
                        s.IsArchived,
                        (SELECT COUNT(*)
                         FROM SprintLearningObjectives slo
                         INNER JOIN LearningObjectives lo ON slo.LearningObjectiveId = lo.Id
                         WHERE slo.SprintId = s.Id AND lo.Archived = 0 AND LOWER(ISNULL(lo.Name, '')) NOT LIKE '%old%') AS LoNumber
                    FROM Sprints s";

                if (hierarchyWhereClauses.Count > 0)
                {
                    sprintsSql += SprintHierarchyJoinSql;
                }

                var whereClauses = new List<string>();
                if (archived.HasValue)
                {
                    whereClauses.Add("s.IsArchived = @Archived");
                }
                whereClauses.AddRange(hierarchyWhereClauses);

                if (whereClauses.Count > 0)
                {
                    sprintsSql += " WHERE " + string.Join(" AND ", whereClauses);
                }

                var sprintRows = (await connection.QueryAsync<SprintBasicRow>(
                    sprintsSql,
                    new
                    {
                        Archived = archived ?? false,
                        hierarchyFilter?.YearName,
                        hierarchyFilter?.ProjectName,
                        hierarchyFilter?.TermName,
                        hierarchyFilter?.SubjectGroupName,
                    })).ToList();

                if (sprintRows.Count == 0)
                {
                    var emptyResult = new List<SprintDTO>();

                    // Cache the empty result
                    var emptyCacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheExpirationMinutes));
                    _cache.Set(cacheKey, emptyResult, emptyCacheOptions);

                    return new ResponseService<List<SprintDTO>>() { Data = emptyResult };
                }

                var sprintIds = sprintRows.Select(s => s.Id).ToList();
                var (expectedTasksDict, completedTasksDict) = await GetSprintProgressAsync(connection, sprintIds);
                var projectNamesBySprint = await GetSprintProjectNamesAsync(connection, sprintIds);

                // Build the result list
                var sprintDtos = sprintRows.Select(sprint =>
                {
                    var totalExpectedTasks = expectedTasksDict.TryGetValue(sprint.Id, out var expected) ? expected : 0;
                    var completedCount = completedTasksDict.TryGetValue(sprint.Id, out var completed) ? completed : 0;

                    return new SprintDTO
                    {
                        Id = sprint.Id,
                        Name = sprint.Name,
                        Description = sprint.Description,
                        StartDate = sprint.StartDate,
                        EndDate = sprint.EndDate,
                        IsArchived = sprint.IsArchived,
                        LoNumber = sprint.LoNumber,
                        CompletePercintag = totalExpectedTasks > 0
                            ? Math.Round((double)completedCount / totalExpectedTasks * 100, 2)
                            : 0,
                        ProjectNames = projectNamesBySprint.TryGetValue(sprint.Id, out var names)
                            ? names
                            : new List<string>()
                    };
                }).ToList();

                if (!string.IsNullOrWhiteSpace(hierarchyFilter?.ProjectName))
                {
                    sprintDtos = sprintDtos
                        .Where(sprint => SprintMatchesProject(sprint, hierarchyFilter.ProjectName))
                        .ToList();
                }

                // Cache the result
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheExpirationMinutes));
                _cache.Set(cacheKey, sprintDtos, cacheEntryOptions);

                return new ResponseService<List<SprintDTO>>() { Data = sprintDtos };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllSprints: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return new ResponseService<List<SprintDTO>>() { Error = true, Message = "Unexpected error" };
            }
        }

        private async Task<Dictionary<int, List<string>>> GetSprintProjectNamesAsync(
            SqlConnection connection,
            List<int> sprintIds)
        {
            if (sprintIds.Count == 0)
            {
                return new Dictionary<int, List<string>>();
            }

            const string sql = @"
                SELECT DISTINCT
                    slo.SprintId,
                    cp.Name AS ProjectName
                FROM SprintLearningObjectives slo
                INNER JOIN LearningObjectives lo ON slo.LearningObjectiveId = lo.Id AND lo.Archived = 0
                INNER JOIN Lessons les ON lo.LessonId = les.Id
                INNER JOIN Units u ON les.UnitId = u.Id
                INNER JOIN Subjects sub ON u.SubjectId = sub.Id
                INNER JOIN SubjectGroups sg ON sub.SubjectGroupId = sg.Id
                INNER JOIN CurriculumTerms ct ON sg.TermId = ct.Id
                INNER JOIN CurriculumProjects cp ON ct.ProjectId = cp.Id
                WHERE slo.SprintId IN @SprintIds
                  AND LOWER(ISNULL(lo.Name, '')) NOT LIKE '%old%'";

            var rows = await connection.QueryAsync<SprintProjectNameRow>(
                sql,
                new { SprintIds = sprintIds });

            return rows
                .GroupBy(row => row.SprintId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(row => (row.ProjectName ?? "").Trim())
                        .Where(name => name.Length > 0)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(name => name)
                        .ToList());
        }

        private async Task<(Dictionary<int, int> Expected, Dictionary<int, int> Completed)> GetSprintProgressAsync(
            SqlConnection connection,
            List<int> sprintIds)
        {
            var expected = new Dictionary<int, int>();
            var completed = new Dictionary<int, int>();

            if (sprintIds.Count == 0)
            {
                return (expected, completed);
            }

            const string expectedTasksSql = @"
                SELECT
                    slo.SprintId,
                    COUNT(st.Id) AS TotalExpectedTasks
                FROM SprintLearningObjectives slo
                INNER JOIN LearningObjectives lo ON slo.LearningObjectiveId = lo.Id AND lo.Archived = 0
                INNER JOIN Schemas sch ON lo.SchemaId = sch.Id
                INNER JOIN Nodes n ON n.SchemaId = sch.Id AND n.Archived = 0
                INNER JOIN Steps st ON st.NodeId = n.Id AND st.Archived = 0
                WHERE slo.SprintId IN @SprintIds
                  AND LOWER(ISNULL(lo.Name, '')) NOT LIKE '%old%'
                GROUP BY slo.SprintId";

            const string completedTasksSql = @"
                SELECT
                    slo.SprintId,
                    COUNT(1) AS CompletedCount
                FROM SprintLearningObjectives slo
                INNER JOIN LearningObjectives lo ON lo.Id = slo.LearningObjectiveId AND lo.Archived = 0
                INNER JOIN Tasks t ON t.LearningObjectiveId = lo.Id AND t.Archived = 0 AND t.Status = 3
                WHERE slo.SprintId IN @SprintIds
                  AND LOWER(ISNULL(lo.Name, '')) NOT LIKE '%old%'
                GROUP BY slo.SprintId";

            try
            {
                var expectedRows = await connection.QueryAsync<SprintExpectedTasksRow>(
                    expectedTasksSql,
                    new { SprintIds = sprintIds },
                    commandTimeout: ProgressQueryTimeoutSeconds);
                expected = expectedRows.ToDictionary(x => x.SprintId, x => x.TotalExpectedTasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetAllSprints progress (expected tasks) skipped: {ex.Message}");
            }

            try
            {
                var completedRows = await connection.QueryAsync<SprintCompletedTasksRow>(
                    completedTasksSql,
                    new { SprintIds = sprintIds },
                    commandTimeout: ProgressQueryTimeoutSeconds);
                completed = completedRows.ToDictionary(x => x.SprintId, x => x.CompletedCount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetAllSprints progress (completed tasks) skipped: {ex.Message}");
            }

            return (expected, completed);
        }

        // Helper DTOs for Dapper query results
        private class SprintBasicRow
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public bool IsArchived { get; set; }
            public int LoNumber { get; set; }
        }

        private class SprintExpectedTasksRow
        {
            public int SprintId { get; set; }
            public int TotalExpectedTasks { get; set; }
        }

        private class SprintCompletedTasksRow
        {
            public int SprintId { get; set; }
            public int CompletedCount { get; set; }
        }

        private class SprintProjectNameRow
        {
            public int SprintId { get; set; }
            public string ProjectName { get; set; } = "";
        }

        public async Task<ResponseService<SprintDTO>> GetSingleSprintAsync(int id)
        {
            var response = new ResponseService<SprintDTO>();

            try
            {
                var sprint = await dataContext.Sprints
                .Include(s => s.SprintLearningObjectives
                    // Filter the join table entries to only those where the LO is not archived
                    .Where(slo => !slo.LearningObjective.Archived))
                    .ThenInclude(slo => slo.LearningObjective) // Pull in the actual LearningObjective
                .FirstOrDefaultAsync(s => s.Id == id);

                if (sprint == null)
                {
                    response.Error = true;
                    response.Message = "This sprint does not exist.";
                    return response;
                }

                var loIds = sprint.SprintLearningObjectives
                    .Select(slo => slo.LearningObjectiveId)
                    .ToList();

                var scopePaths = loIds.Count == 0
                    ? new List<string>()
                    : await dataContext.LearningObjectives
                        .AsNoTracking()
                        .Where(lo => loIds.Contains(lo.Id))
                        .Select(lo =>
                            lo.Lesson.Unit.Subject.SubjectGroup.Term.Project.Year.Name + " > " +
                            lo.Lesson.Unit.Subject.SubjectGroup.Term.Project.Name + " > " +
                            lo.Lesson.Unit.Subject.SubjectGroup.Term.Name + " > " +
                            lo.Lesson.Unit.Subject.SubjectGroup.Name)
                        .Distinct()
                        .ToListAsync();

                var projectNames = loIds.Count == 0
                    ? new List<string>()
                    : await dataContext.LearningObjectives
                        .AsNoTracking()
                        .Where(lo => loIds.Contains(lo.Id))
                        .Select(lo => lo.Lesson.Unit.Subject.SubjectGroup.Term.Project.Name)
                        .Distinct()
                        .OrderBy(name => name)
                        .ToListAsync();

                // Map the sprint and its associated learning objectives to the DTO
                response.Data = new SprintDTO
                {
                    Id = sprint.Id,
                    Name = sprint.Name,
                    Description = sprint.Description,
                    StartDate = sprint.StartDate,
                    EndDate = sprint.EndDate,
                    IsArchived = sprint.IsArchived,
                    ProjectNames = projectNames,
                    ScopeFolderPath = scopePaths.Count == 1 ? scopePaths[0] : null,
                    // Project the LearningObjectives from the join table
                    // This is safer than assuming a direct `sprint.LearningObjectives` if not explicitly configured as a skip navigation.
                    learningObjects = sprint.SprintLearningObjectives
                                            .Select(slo => slo.LearningObjective) // Get the LearningObjective from each join entry
                                            .Where(lo => lo != null) // Ensure the LO was loaded successfully (should be with ThenInclude)
                                            .Where(lo => lo!.Name == null || !lo.Name.Contains("old", StringComparison.OrdinalIgnoreCase))
                                            .Select(lo => new Responses.IDName
                                            {
                                                Id = lo.Id,
                                                Name = lo.Name,
                                            }).ToList()
                };

                response.Message = "Sprint fetched successfully.";
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes (use a proper logger in production)
                Console.WriteLine($"Error in GetSingleSprintAsync: {ex.Message}");
                Console.WriteLine(ex.StackTrace);

                response.Error = true;
                response.Message = $"An unexpected error occurred: {ex.Message}";
            }

            return response;
        }

        public async Task<ResponseService<Responses.SprintDto>> CreateNewSprintAsync(Request.CreateSprint request)
        {
            var response = new ResponseService<Responses.SprintDto>();

            try
            {
                // Parse dates from string to DateOnly for overlap check
                var startDateOnly = DateOnly.Parse(request.StartDate);
                var endDateOnly = DateOnly.Parse(request.EndDate);

                // Input validation: Ensure StartDate is not after EndDate
                if (startDateOnly > endDateOnly)
                {
                    response.Error = true;
                    response.Message = "Start Date cannot be after End Date.";
                    return response;
                }

                //if (await IsOverlappingAsync(startDateOnly, endDateOnly))
                //{
                //    response.Error = true;
                //    response.Message = "There is another sprint within this date range.";
                //    return response;
                //}

                // Create the new Sprint model
                var sprint = new Models.Sprint // Assuming Models.Sprint is your EF Core entity
                {
                    Name = request.Name,
                    Description = request.Description,
                    StartDate = DateTime.Parse(request.StartDate), 
                    EndDate = DateTime.Parse(request.EndDate)      
                };

                // Add the sprint to the context but don't save yet
                dataContext.Sprints.Add(sprint);

                // --- Associate Learning Objectives ---
                if (request.Los != null && request.Los.Any())
                {
                    var learningObjectiveIdsToAssociate = request.Los.Select(lo => lo.Id).ToList();

                    var existingLearningObjectives = await dataContext.LearningObjectives
                        .Where(lo => learningObjectiveIdsToAssociate.Contains(lo.Id))
                        .ToListAsync();

                    if (existingLearningObjectives.Count != learningObjectiveIdsToAssociate.Count)
                    {
                        // This is good validation to ensure all requested LOs exist
                        response.Error = true;
                        response.Message = "One or more specified Learning Objectives were not found.";
                        // You might want to remove the newly added sprint if you don't want partial creation
                        // dataContext.Sprints.Remove(sprint);
                        return response;
                    }

                    var scopeError = await ValidateSingleProjectScopeAsync(learningObjectiveIdsToAssociate);
                    if (scopeError != null)
                    {
                        response.Error = true;
                        response.Message = scopeError;
                        return response;
                    }

                    // Create SprintLearningObjective entries for the many-to-many relationship
                    foreach (var lo in existingLearningObjectives)
                    {
                        var sprintLo = new Models.SprintLearningObjective
                        {
                            Sprint = sprint, // Link to the new sprint entity
                            LearningObjective = lo // Link to the existing learning objective entity
                                                   // EF Core will automatically handle SprintId and LearningObjectiveId when SaveChanges is called
                                                   // because it tracks the Sprint and LearningObjective entities.
                        };
                        // Add to the sprint's navigation collection
                        sprint.SprintLearningObjectives.Add(sprintLo);
                    }
                }

                // Save all changes (new sprint and updated learning objectives) to the database
                await dataContext.SaveChangesAsync();
                InvalidateAllSprintsCache();

                // Prepare the successful response DTO
                response.Data = new Responses.SprintDto
                {
                    Id = sprint.Id, 
                    Name = sprint.Name,
                    Description = sprint.Description,
                    StartDate = DateOnly.FromDateTime(sprint.StartDate).ToString(),
                    EndDate = DateOnly.FromDateTime(sprint.EndDate).ToString()
                };

                response.Message = "Sprint created successfully and learning objectives associated.";
            }
            catch (FormatException)
            {
                response.Error = true;
                response.Message = "Invalid date format. Please use a valid date string (e.g., 'YYYY-MM-DD').";
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                // Consider using a proper logging framework (e.g., Serilog, NLog)
                Console.WriteLine($"Error in CreateNewSprintAsync: {ex.Message}");
                Console.WriteLine(ex.StackTrace);

                response.Error = true;
                response.Message = $"An unexpected error occurred: {ex.Message}";
            }

            return response;
        }

        /// <summary>
        /// Updates an existing sprint with new details and manages its associated learning objectives.
        /// </summary>
        /// <param name="sprintId">The ID of the sprint to update.</param>
        /// <param name="request">The request body containing the updated sprint information.</param>
        /// <returns>A ResponseService indicating the success or failure of the update operation.</returns>
        public async Task<ResponseService<Responses.SprintDto>> UpdateSprintAsync(int sprintId,Request.UpdateSprint request)
        {
            var response = new ResponseService<Responses.SprintDto>();

            try
            {
                // 1. Find the existing sprint
                var sprintToUpdate = await dataContext.Sprints
                                                        .Include(s => s.SprintLearningObjectives) // Include for managing LOs
                                                            .ThenInclude(slo => slo.LearningObjective) // Include actual LO data for response DTO
                                                        .FirstOrDefaultAsync(s => s.Id == sprintId);

                if (sprintToUpdate == null)
                {
                    response.Error = true;
                    response.Message = "Sprint not found.";
                    return response;
                }

                // 2. Parse and validate dates
                // Using DateOnly for comparison, then converting to DateTime for storing in model
                var newStartDateOnly = DateOnly.Parse(request.StartDate);
                var newEndDateOnly = DateOnly.Parse(request.EndDate);

                if (newStartDateOnly > newEndDateOnly)
                {
                    response.Error = true;
                    response.Message = "Start Date cannot be after End Date.";
                    return response;
                }

                // Check for overlaps, excluding the current sprint being updated
                //if (await IsOverlappingAsync(newStartDateOnly, newEndDateOnly,sprintId))
                //{
                //    response.Error = true;
                //    response.Message = "The updated date range overlaps with another existing sprint.";
                //    return response;
                //}

                // 3. Update basic sprint properties
                sprintToUpdate.Name = request.Name;
                sprintToUpdate.Description = request.Description;
                sprintToUpdate.StartDate = DateTime.Parse(request.StartDate); // Convert back to DateTime for storage
                sprintToUpdate.EndDate = DateTime.Parse(request.EndDate);     // Convert back to DateTime for storage

                // 4. Manage Learning Objectives (many-to-many relationship)
                var currentLoIds = sprintToUpdate.SprintLearningObjectives.Select(slo => slo.LearningObjectiveId).ToList();
                var requestedLoIds = request.Los ?? new List<int>();

                var scopeError = await ValidateSingleProjectScopeAsync(requestedLoIds);
                if (scopeError != null)
                {
                    response.Error = true;
                    response.Message = scopeError;
                    return response;
                }

                // LOs to remove: In current but not in requested
                var loIdsToRemove = currentLoIds.Except(requestedLoIds).ToList();
                foreach (var loIdToRemove in loIdsToRemove)
                {
                    var sprintLoToRemove = sprintToUpdate.SprintLearningObjectives
                                                         .FirstOrDefault(slo => slo.LearningObjectiveId == loIdToRemove);
                    if (sprintLoToRemove != null)
                    {
                        dataContext.SprintLearningObjectives.Remove(sprintLoToRemove);
                    }
                }

                // LOs to add: In requested but not in current
                var loIdsToAdd = requestedLoIds.Except(currentLoIds).ToList();
                if (loIdsToAdd.Any())
                {
                    var learningObjectivesToAdd = await dataContext.LearningObjectives
                        .Where(lo => loIdsToAdd.Contains(lo.Id))
                        .ToListAsync();

                    if (learningObjectivesToAdd.Count != loIdsToAdd.Count)
                    {
                        response.Error = true;
                        response.Message = "One or more new Learning Objectives specified were not found.";
                        return response;
                    }

                    foreach (var lo in learningObjectivesToAdd)
                    {
                        sprintToUpdate.SprintLearningObjectives.Add(new Models.SprintLearningObjective
                        {
                            Sprint = sprintToUpdate,
                            LearningObjective = lo
                        });
                    }
                }

                // 5. Save all changes to the database
                await dataContext.SaveChangesAsync();
                InvalidateAllSprintsCache();

                // 6. Prepare the successful response DTO
                response.Data = new Responses.SprintDto
                {
                    Id = sprintToUpdate.Id,
                    Name = sprintToUpdate.Name,
                    Description = sprintToUpdate.Description,
                    StartDate = sprintToUpdate.StartDate.ToString(),
                    EndDate = sprintToUpdate.EndDate.ToString()
                };
                response.Message = "Sprint updated successfully and learning objectives managed.";
            }
            catch (FormatException)
            {
                response.Error = true;
                response.Message = "Invalid date format. Please use a valid date string (e.g., 'MM/DD/YYYY' or 'YYYY-MM-DD').";
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"Error in UpdateSprintAsync: {ex.Message}");
                Console.WriteLine(ex.StackTrace);

                response.Error = true;
                response.Message = $"An unexpected error occurred: {ex.Message}";
            }

            return response;
        }

        public async Task<ActionResult<BaseResponseService>> DeleteSprint(int id)
        {
            var response = new BaseResponseService();
            try
            {
                var sprint = await dataContext.Sprints.FirstOrDefaultAsync(x => x.Id == id);

                if (sprint == null) return new BaseResponseService() { Error = true, Message = "this sprint is not exist" };

                dataContext.Remove(sprint);
                await dataContext.SaveChangesAsync();

                response.Message = "Sprint removed successfully.";
            }
            catch (Exception ex)
            {
                response.Error = true;
                response.Message = $"An error occurred: {ex.Message}";
            }


            return response;
        }

        private Task<bool> IsOverlappingAsync(DateOnly startDate, DateOnly endDate)
        {
            // Delegate to the main implementation, with no current sprint to exclude
            return IsOverlappingAsync(startDate, endDate, null);
        }

        /// <summary>
        /// Checks if a given date range for a sprint overlaps with any other existing sprints.
        /// When updating a sprint, the ID of the current sprint can be provided to exclude it
        /// from the overlap check, preventing a sprint from overlapping with itself.
        /// </summary>
        /// <param name="newStartDate">The start date of the new or updated sprint's range.</param>
        /// <param name="newEndDate">The end date of the new or updated sprint's range.</param>
        /// <param name="currentSprintId">Optional: The ID of the sprint currently being updated.
        /// If provided, this sprint will be excluded from the overlap check.</param>
        /// <returns>True if an overlap is found, false otherwise.</returns>
        private async Task<bool> IsOverlappingAsync(DateOnly newStartDate, DateOnly newEndDate, int? currentSprintId = null)
        {
            // Convert DateOnly to DateTime at the start of each day for comparison with stored DateTime values.
            var start = newStartDate.ToDateTime(TimeOnly.MinValue);
            var end = newEndDate.ToDateTime(TimeOnly.MinValue);

            // Overlap condition: an existing (non-archived) sprint overlaps if
            // existing.StartDate <= newEnd AND existing.EndDate >= newStart.
            // If currentSprintId is provided (for updates), exclude that sprint from the check.
            var isOverlap = await dataContext.Sprints
                .AnyAsync(s => !s.IsArchived &&
                               (currentSprintId == null || s.Id != currentSprintId.Value) &&
                               start <= s.EndDate &&
                               end >= s.StartDate);

            return isOverlap;
        }

        /// <summary>
        /// Archives or unarchives a sprint.
        /// </summary>
        /// <param name="sprintId">The ID of the sprint to archive/unarchive.</param>
        /// <param name="archived">True to archive, false to unarchive.</param>
        /// <returns>A ResponseService indicating the success or failure of the operation.</returns>
        public async Task<ResponseService<Responses.SprintDto>> ArchiveSprintAsync(int sprintId, bool archived)
        {
            var response = new ResponseService<Responses.SprintDto>();

            try
            {
                var sprint = await dataContext.Sprints.FindAsync(sprintId);

                if (sprint == null)
                {
                    response.Error = true;
                    response.Message = "Sprint not found.";
                    return response;
                }

                sprint.IsArchived = archived;
                await dataContext.SaveChangesAsync();
                InvalidateAllSprintsCache();

                response.Data = new Responses.SprintDto
                {
                    Id = sprint.Id,
                    Name = sprint.Name,
                    Description = sprint.Description,
                    StartDate = DateOnly.FromDateTime(sprint.StartDate).ToString(),
                    EndDate = DateOnly.FromDateTime(sprint.EndDate).ToString(),
                    IsArchived = sprint.IsArchived
                };

                response.Message = archived ? "Sprint archived successfully." : "Sprint unarchived successfully.";
            }
            catch (Exception ex)
            {
                response.Error = true;
                response.Message = $"An unexpected error occurred: {ex.Message}";
            }

            return response;
        }

        private const int MaxResolveLoNames = 500;

        private static bool IsOldLearningObjectiveName(string? name) =>
            !string.IsNullOrWhiteSpace(name) && name.Contains("old", StringComparison.OrdinalIgnoreCase);

        public async Task<ResponseService<ResolveLosByNameResult>> ResolveLosByNameAsync(Request.ResolveLosByName request)
        {
            var result = new ResolveLosByNameResult();
            var response = new ResponseService<ResolveLosByNameResult> { Data = result };

            var requestedNames = (request.Names ?? new List<string>())
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (requestedNames.Count == 0)
            {
                response.Message = "No learning objective names were provided.";
                return response;
            }

            if (requestedNames.Count > MaxResolveLoNames)
            {
                response.Error = true;
                response.Message = $"A maximum of {MaxResolveLoNames} learning objective names can be imported at once.";
                return response;
            }

            try
            {
                var candidates = await dataContext.LearningObjectives
                    .Include(lo => lo.Lesson)
                        .ThenInclude(l => l.Unit)
                            .ThenInclude(u => u.Subject)
                    .Where(lo => requestedNames.Contains(lo.Name))
                    .ToListAsync();

                var byName = candidates
                    .GroupBy(lo => (lo.Name ?? "").Trim(), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

                foreach (var name in requestedNames)
                {
                    if (!byName.TryGetValue(name, out var matches) || matches.Count == 0)
                    {
                        result.Errors.Add(new LoNameError
                        {
                            Name = name,
                            Message = "Learning objective does not exist"
                        });
                        continue;
                    }

                    var eligible = matches.Where(lo => GetIneligibilityReason(lo) == null).ToList();
                    if (eligible.Count == 1)
                    {
                        var lo = eligible[0];
                        result.Matched.Add(new Responses.IDName { Id = lo.Id, Name = lo.Name });
                        continue;
                    }

                    if (eligible.Count > 1)
                    {
                        result.Errors.Add(new LoNameError
                        {
                            Name = name,
                            Message = "Name matches more than one learning objective"
                        });
                        continue;
                    }

                    result.Errors.Add(new LoNameError
                    {
                        Name = name,
                        Message = matches
                            .Select(GetIneligibilityReason)
                            .Where(reason => reason != null)
                            .OrderBy(reason => IneligibilityRank(reason!))
                            .First()!
                    });
                }

                response.Message = result.Errors.Count == 0
                    ? "Learning objectives resolved successfully."
                    : "Some learning objective names could not be resolved.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ResolveLosByNameAsync: {ex.Message}");
                response.Error = true;
                response.Message = $"An unexpected error occurred: {ex.Message}";
            }

            return response;
        }

        private static string? GetIneligibilityReason(LearningObjective lo)
        {
            if (IsOldLearningObjectiveName(lo.Name))
                return "Learning objective is marked old and cannot be added to a sprint";
            if (lo.Archived)
                return "Learning objective is archived";
            if (lo.DoneAt != null)
                return "Learning objective is already completed";

            var lesson = lo.Lesson;
            var unit = lesson?.Unit;
            var subject = unit?.Subject;
            if (lesson == null || lesson.Archived || unit == null || unit.Archived
                || subject == null || subject.Archived || subject.ArchivedWithFolder)
            {
                return "Learning objective is not available (archived parent)";
            }

            if (subject.Status == ProjectStatusEnum.Hold || subject.Status == ProjectStatusEnum.Closed)
                return "Subject is on hold or closed";

            return null;
        }

        private static int IneligibilityRank(string reason) => reason switch
        {
            "Learning objective is archived" => 0,
            "Learning objective is marked old and cannot be added to a sprint" => 1,
            "Learning objective is already completed" => 2,
            "Learning objective is not available (archived parent)" => 3,
            "Subject is on hold or closed" => 4,
            _ => 5
        };
    }
}