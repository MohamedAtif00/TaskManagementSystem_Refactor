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
                var sprint = await dataContext.Sprints.FirstOrDefaultAsync(x => x.Id == id);

                if (sprint == null)
                {
                    response.Error = true;
                    response.Message = "This sprint does not exist.";
                    return response;
                }

                response.Data = new SprintDTO
                {
                    Id = sprint.Id,
                    Name = sprint.Name,
                    Description = sprint.Description,
                    StartDate = sprint.StartDate,
                    EndDate = sprint.EndDate
                };

                response.Message = "Sprint fetched successfully.";
            }
            catch (Exception)
            {
                response.Error = true;
                response.Message = "Unexpected error occurred.";
            }

            return response;
        }

        public async Task<ResponseService<Responses.SprintDto>> CreateNewSprintAsync(Request.CreateSprint request)
        {
            var response = new ResponseService<Responses.SprintDto>();

            try
            {
                var startDate = DateOnly.Parse(request.StartDate);
                var endDate = DateOnly.Parse(request.EndDate);

                if (await IsOverlappingAsync(startDate, endDate))
                {
                    response.Error = true;
                    response.Message = "There is another sprint within this date range.";
                    return response;
                }

                var sprint = new Models.Sprint
                {
                    Name = request.Name,
                    Description = request.Description,
                    StartDate = DateTime.Parse(request.StartDate),
                    EndDate = DateTime.Parse(request.EndDate)
                };

                dataContext.Sprints.Add(sprint);
                await dataContext.SaveChangesAsync();

                response.Data = new Responses.SprintDto
                {
                    Id = sprint.Id,
                    Name = sprint.Name,
                    Description = sprint.Description,
                    StartDate = DateOnly.FromDateTime(sprint.StartDate),
                    EndDate = DateOnly.FromDateTime(sprint.EndDate)
                };
                response.Message = "Sprint created successfully.";
            }
            catch (Exception ex)
            {
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