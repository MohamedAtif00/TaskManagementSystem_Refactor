using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Dtos.SprintDtos;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.Sprint
{
    public interface ISprintService
    {
        Task<ResponseService<Responses.SprintDto>> CreateNewSprintAsync(Request.CreateSprint request);
        Task<ActionResult<BaseResponseService>> DeleteSprint(int id);
        Task<ActionResult<ResponseService<List<SprintDTO>>>> GetAllSprints();
        Task<ResponseService<SprintDTO>> GetSingleSprintAsync(int id);
    }
}