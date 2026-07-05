using System.Data;
using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Dtos.SprintDtos;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums.TaskStatus;
using AutomatedTaskSystem.Services.ResponseService;
using Dapper;
using Microsoft.AspNetCore.Mvc;
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

        public SprintService(DataContext dataContext, IMemoryCache cache)
        {
            this.dataContext = dataContext;
            _cache = cache;
        }

        /// <summary>
        /// Generates a cache key for all sprints based on archived filter
        /// </summary>
        private static string GetAllSprintsCacheKey(bool? archived)
        {
            if (archived == null) return $"{AllSprintsCacheKeyPrefix}All";
            return archived.Value ? $"{AllSprintsCacheKeyPrefix}Archived" : $"{AllSprintsCacheKeyPrefix}Active";
        }

        /// <summary>
        /// Optimized version using Dapper for improved performance with caching.
        /// Replaces EF Core with direct SQL queries to minimize database round trips.
        /// Results are cached for improved performance on subsequent calls.
        /// </summary>
        public async Task<ResponseService<List<SprintDTO>>> GetAllSprints(bool? archived = null)
        {
            try
            {
                // Try to get from cache first
                var cacheKey = GetAllSprintsCacheKey(archived);
                if (_cache.TryGetValue(cacheKey, out List<SprintDTO>? cachedData) && cachedData != null)
                {
                    return new ResponseService<List<SprintDTO>>() { Data = cachedData };
                }

                var connection = dataContext.Database.GetDbConnection();
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                // Query 1: Get all sprints with basic info and LO count
                var sprintsSql = @"
                    SELECT
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

                if (archived.HasValue)
                {
                    sprintsSql += " WHERE s.IsArchived = @Archived";
                }

                var sprintRows = (await connection.QueryAsync<SprintBasicRow>(
                    sprintsSql,
                    new { Archived = archived ?? false })).ToList();

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

                // Query 2: Get total expected tasks (schema steps) per sprint
                const string expectedTasksSql = @"
                    SELECT
                        slo.SprintId,
                        COUNT(st.Id) AS TotalExpectedTasks
                    FROM SprintLearningObjectives slo
                    INNER JOIN LearningObjectives lo ON slo.LearningObjectiveId = lo.Id
                    INNER JOIN Schemas sch ON lo.SchemaId = sch.Id
                    INNER JOIN Nodes n ON n.SchemaId = sch.Id
                    INNER JOIN Steps st ON st.NodeId = n.Id
                    WHERE slo.SprintId IN @SprintIds
                      AND lo.Archived = 0
                      AND LOWER(ISNULL(lo.Name, '')) NOT LIKE '%old%'
                      AND n.Archived = 0
                      AND st.Archived = 0
                    GROUP BY slo.SprintId";

                var expectedTasksDict = (await connection.QueryAsync<SprintExpectedTasksRow>(
                    expectedTasksSql,
                    new { SprintIds = sprintIds }))
                    .ToDictionary(x => x.SprintId, x => x.TotalExpectedTasks);

                // Query 3: Get completed task count per sprint
                const string completedTasksSql = @"
                    SELECT
                        slo.SprintId,
                        COUNT(t.Id) AS CompletedCount
                    FROM SprintLearningObjectives slo
                    INNER JOIN LearningObjectives lo ON slo.LearningObjectiveId = lo.Id
                    INNER JOIN Tasks t ON t.LearningObjectiveId = lo.Id
                    WHERE slo.SprintId IN @SprintIds
                      AND lo.Archived = 0
                      AND LOWER(ISNULL(lo.Name, '')) NOT LIKE '%old%'
                      AND t.Archived = 0
                      AND t.Status = 3  -- TaskStatusEnum.Done = 3
                    GROUP BY slo.SprintId";

                var completedTasksDict = (await connection.QueryAsync<SprintCompletedTasksRow>(
                    completedTasksSql,
                    new { SprintIds = sprintIds }))
                    .ToDictionary(x => x.SprintId, x => x.CompletedCount);

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
                            : 0
                    };
                }).ToList();

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

                // Map the sprint and its associated learning objectives to the DTO
                response.Data = new SprintDTO
                {
                    Id = sprint.Id,
                    Name = sprint.Name,
                    Description = sprint.Description,
                    StartDate = sprint.StartDate,
                    EndDate = sprint.EndDate,
                    IsArchived = sprint.IsArchived,
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





    }
}