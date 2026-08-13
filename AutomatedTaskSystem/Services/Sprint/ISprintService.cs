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
        Task<ResponseService<List<SprintDTO>>> GetAllSprints(bool? archived = null, SprintHierarchyFilter? hierarchyFilter = null);
        Task<ResponseService<SprintDTO>> GetSingleSprintAsync(int id);
        Task<ResponseService<Responses.SprintDto>> UpdateSprintAsync(int sprintId, Request.UpdateSprint request);
        Task<ResponseService<Responses.SprintDto>> ArchiveSprintAsync(int sprintId, bool archived);
        Task<ResponseService<ResolveLosByNameResult>> ResolveLosByNameAsync(Request.ResolveLosByName request);
    }
}