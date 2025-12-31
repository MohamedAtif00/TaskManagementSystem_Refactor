using AutomatedTaskSystem.Data;

using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Dtos.SprintDtos;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutomatedTaskSystem.Services.Sprint
{
    public class SprintService : ISprintService
    {

        private readonly DataContext dataContext;
        public SprintService(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        public async Task<ResponseService<List<SprintDTO>>> GetAllSprints(bool? archived = null)
        {
            try
            {
                IQueryable<Models.Sprint> query = dataContext.Sprints;

                // Filter by archived status if specified
                if (archived.HasValue)
                {
                    query = query.Where(s => s.IsArchived == archived.Value);
                }

                List<Models.Sprint> sprints = await query.ToListAsync();
                List<SprintDTO> sprintDto = new();

                foreach (var sprint in sprints)
                {
                    if (sprint.IsArchived) continue;
                    sprintDto.Add(new SprintDTO
                    {
                        Id = sprint.Id,
                        Name = sprint.Name,
                        Description = sprint.Description,
                        StartDate = sprint.StartDate,
                        EndDate = sprint.EndDate,
                        IsArchived = sprint.IsArchived
                    });
                }

                return new ResponseService<List<SprintDTO>>() { Data = sprintDto };
            }
            catch (Exception ex)
            {
                return new ResponseService<List<SprintDTO>>() { Error = true, Message = "Unexcepected error" };
            }
        }

        public async Task<ResponseService<SprintDTO>> GetSingleSprintAsync(int id)
        {
            var response = new ResponseService<SprintDTO>();

            try
            {
                var sprint = await dataContext.Sprints
                    .Include(s => s.SprintLearningObjectives) // Include the join table entries
                        .ThenInclude(slo => slo.LearningObjective) // Then include the actual LearningObjective from the join table
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

                if (await IsOverlappingAsync(startDateOnly, endDateOnly))
                {
                    response.Error = true;
                    response.Message = "There is another sprint within this date range.";
                    return response;
                }

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
                if (await IsOverlappingAsync(newStartDateOnly, newEndDateOnly,sprintId))
                {
                    response.Error = true;
                    response.Message = "The updated date range overlaps with another existing sprint.";
                    return response;
                }

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

        private async Task<bool> IsOverlappingAsync(DateOnly startDate, DateOnly endDate)
        {
            // Convert DateOnly to DateTime
            var start = startDate.ToDateTime(TimeOnly.MinValue).Date;  // Extract only the Date portion
            var end = endDate.ToDateTime(TimeOnly.MaxValue).Date;      // Extract only the Date portion

            var condition = await dataContext.Sprints.AnyAsync(s =>
                (start <= s.StartDate.Date && end >= s.StartDate.Date) ||  // Check if the new range starts before an existing start and ends after an existing start
                (start <= s.EndDate.Date && end >= s.EndDate.Date) ||
                (start >= s.StartDate.Date && end <= s.EndDate.Date) ||
                start == s.StartDate.Date || end == s.EndDate.Date// Check if the new range starts before an existing end and ends after an existing end
            );

            return condition;
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
            // Convert DateOnly to DateTime for comparison with existing DateTime properties in Sprint model.
            // Using .Date to ensure only the date portion is compared, ignoring time.
            var start = newStartDate.ToDateTime(TimeOnly.MinValue).Date;
            var end = newEndDate.ToDateTime(TimeOnly.MaxValue).Date;

            // Query for any existing sprints that overlap with the new date range.
            // The condition for overlap is: (StartDate <= newEndDate AND EndDate >= newStartDate)
            // Additionally, if currentSprintId is provided (for updates),
            // we ensure that the queried sprint's ID is NOT the currentSprintId.
            var isOverlap = await dataContext.Sprints
                .AnyAsync(s => (currentSprintId == null || s.Id != currentSprintId.Value) && // Exclude the current sprint if ID is provided
                               (start <= s.EndDate.Date && end >= s.StartDate.Date)); // Check for overlap condition

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