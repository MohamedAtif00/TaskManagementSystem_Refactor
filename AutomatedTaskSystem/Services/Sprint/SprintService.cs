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

        public async Task<ActionResult<ResponseService<List<SprintDTO>>>> GetAllSprints()
        {
            try
            {
                List<Models.Sprint> sprints = await dataContext.Sprints.ToListAsync();
                List<SprintDTO> sprintDto = new();

                foreach (var sprint in sprints)
                {
                    sprintDto.Add(new SprintDTO
                    {
                        Id = sprint.Id,
                        Name = sprint.Name,
                        Description = sprint.Description,
                        StartDate = sprint.StartDate,
                        EndDate = sprint.EndDate
                    });
                }

                return new ResponseService<List<SprintDTO>>() { Data = sprintDto };
            }
            catch (Exception ex)
            {
                return new NotFoundObjectResult(new BaseResponseService() { Error = true, Message = "Unexcepected error" });
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





    }
}