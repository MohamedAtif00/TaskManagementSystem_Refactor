using AutomatedTaskSystem.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutomatedTaskSystem.Services.SubjectService;
using AutomatedTaskSystem.Services.YearService;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Dtos.Projects;
using static AutomatedTaskSystem.DTO.Responses;

namespace AutomatedTaskSystem.Controllers;

[Route("subjects")]
[ApiController]
public class SubjectController : ControllerBase
{
    private readonly ISubjectService _subjectService;
    private readonly IYearService _yearService;

    public SubjectController(ISubjectService subjectService, IYearService yearService)
    {
        _subjectService = subjectService;
        _yearService = yearService;
    }

    [HttpGet("{id}/users/unassigned")]
    public async Task<ActionResult<ResponseService<List<UserDTO>>>> GetUnassignedUsers(int id) =>
        await _subjectService.GetUnassignedUsers(id);

    [HttpGet("{id}/users/assigned")]
    public async Task<ActionResult<ResponseService<List<UserDTO>>>> GetAssignedUsers(int id) =>
        await _subjectService.GetAssignedUsers(id);

    [Authorize]
    [HttpGet("assignment")]
    public async Task<ActionResult<ResponseService<List<SubjectDTO>>>> GetAssignedSubjects() =>
        await _subjectService.GetUserSpecificProjects();

    [HttpPost("{id}/assign")]
    public async Task<ActionResult<ResponseService<List<IDName>>>> AssignToSubject(
        int id,
        Requests.LOAssignDTO req
    ) => await _subjectService.AssignToProject(id, req.UserIds);

    [HttpDelete("{id}")]
    public async Task<ActionResult<BaseResponseService>> DeleteSubject(int id) =>
        await _subjectService.DeleteProject(id);

    [HttpPost("{id}/unassign")]
    public async Task<ActionResult<ResponseService<List<IDName>>>> UnassignFromSubject(
        int id,
        Requests.LOAssignDTO req
    ) => await _subjectService.UnassignToProject(id, req.UserIds);

    [Authorize]
    [HttpGet("{id}/details")]
    public async Task<ActionResult<ResponseService<DetailedProjectDTO>>> GetSubjectDetail(int id) =>
        await _subjectService.GetProjectDetails(id);

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<ResponseService<SubjectDTO>>> GetSubject(int id) =>
        await _subjectService.GetProject(id);

    [HttpGet]
    public async Task<ActionResult<ResponseService<List<SubjectDTO>>>> GetSubjects() =>
        await _subjectService.GetAllProjects();

    [Authorize]
    [HttpGet("by-term/{termId:int}")]
    public async Task<ActionResult<ResponseService<List<SubjectDTO>>>> GetSubjectsByTerm(
        int termId,
        [FromQuery] bool includeInactive = false
    ) => await _subjectService.GetSubjectsByTerm(termId, includeInactive);

    [HttpGet("GetAllForSprint")]
    public async Task<ActionResult<ResponseService<List<SubjectDTO>>>> GetSubjectsForSprint() =>
        await _subjectService.GetAllProjectsForSprint();

    [HttpGet("{id}/los")]
    public async Task<ActionResult<ResponseService<List<IDName>>>> GetSubjectLos(int id) =>
        await _subjectService.GetProjectLearningObjectives(id);

    [HttpPatch("{id}")]
    public async Task<ActionResult<ResponseService<SubjectDTO>>> EditSubject(
        int id,
        Requests.SubjectWriteDTO req
    ) => await _subjectService.EditProject(id, req.Name, req.Description, req.TermId);

    [HttpPost]
    public async Task<ActionResult<ResponseService<SubjectDTO>>> CreateSubject(
        Requests.SubjectWriteDTO req
    ) => await _subjectService.CreateProject(req.Name, req.Description, req.TermId);

    [HttpPost("{id}/units")]
    public async Task<ActionResult<ResponseService<ProjectUnitDTO>>> AddUnit(
        int id,
        Requests.NameDTO req
    ) => await _subjectService.AddUnit(id, req.Name);

    /// <summary>Legacy global years (reference data). Prefer terms under /projects for hierarchy.</summary>
    [HttpGet("years")]
    public async Task<ActionResult<ResponseService<List<IDName>>>> GetActiveYears() =>
        await _yearService.GetActiveYears();

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<ResponseService<SubjectDTO>>> UpdateStatus(
        int id,
        UpdateProjectStatusDto req
    ) => await _subjectService.UpdateProjectStatus(id, req.Status);
}
