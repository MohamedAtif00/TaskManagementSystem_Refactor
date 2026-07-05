using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.SectionService;

public interface ISectionService
{
    Task<ActionResult<ResponseService<Responses.SectionDTO>>> GetSection(int Id);
    Task<ActionResult<ResponseService<Responses.IDName>>> CreateSection(
        string Name,
        int HeadId,
        List<int> Groups
    );
    Task<ActionResult<ResponseService<List<Responses.IDName>>>> GetSections();
    Task<ActionResult<ResponseService<Responses.SectionDTO>>> EditSection(
        int id,
        Requests.SectionDTO request
    );
    Task<ActionResult<BaseResponseService>> DeleteSection(int id);
}
