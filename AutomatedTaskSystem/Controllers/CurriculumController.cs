using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Services.CurriculumService;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Controllers;

[Route("curriculum")]
[ApiController]
public class CurriculumController(ICurriculumService curriculumService) : ControllerBase
{
    [HttpGet("years")]
    public async Task<ActionResult<ResponseService<List<CurriculumNodeDto>>>> GetYears() =>
        ToActionResult(await curriculumService.GetYearsAsync());

    [HttpGet("years/{yearId:int}")]
    public async Task<ActionResult<ResponseService<CurriculumNodeDto>>> GetYear(int yearId) =>
        ToActionResult(await curriculumService.GetYearAsync(yearId));

    [HttpPost("years")]
    public async Task<ActionResult<ResponseService<CurriculumNodeDto>>> CreateYear([FromBody] CreateYearDto body) =>
        ToActionResult(await curriculumService.CreateYearAsync(body.Name, body.Description));

    [HttpPatch("years/{yearId:int}")]
    public async Task<ActionResult<ResponseService<CurriculumNodeDto>>> UpdateYear(int yearId, [FromBody] UpdateYearDto body) =>
        ToActionResult(await curriculumService.UpdateYearAsync(yearId, body.Name, body.Description));

    [HttpDelete("years/{yearId:int}")]
    public async Task<ActionResult<ResponseService<bool>>> DeleteYear(int yearId) =>
        ToActionResult(await curriculumService.DeleteYearAsync(yearId));

    [HttpGet("years/{yearId:int}/tree")]
    public async Task<ActionResult<ResponseService<List<CurriculumNodeDto>>>> GetYearTree(int yearId) =>
        ToActionResult(await curriculumService.GetYearTreeAsync(yearId));

    [HttpGet("years/{yearId:int}/projects")]
    public async Task<ActionResult<ResponseService<List<CurriculumNodeDto>>>> GetProjects(int yearId) =>
        ToActionResult(await curriculumService.GetProjectsAsync(yearId));

    [HttpPost("years/{yearId:int}/projects")]
    public async Task<ActionResult<ResponseService<CurriculumNodeDto>>> CreateProject(int yearId, [FromBody] CreateProjectDto body) =>
        ToActionResult(await curriculumService.CreateProjectAsync(yearId, body.Name, body.Description));

    [HttpGet("projects/{projectId:int}")]
    public async Task<ActionResult<ResponseService<CurriculumNodeDto>>> GetProject(int projectId) =>
        ToActionResult(await curriculumService.GetProjectAsync(projectId));

    [HttpPatch("projects/{projectId:int}")]
    public async Task<ActionResult<ResponseService<CurriculumNodeDto>>> UpdateProject(int projectId, [FromBody] UpdateProjectDto body) =>
        ToActionResult(await curriculumService.UpdateProjectAsync(projectId, body.Name, body.Description));

    [HttpDelete("projects/{projectId:int}")]
    public async Task<ActionResult<ResponseService<bool>>> DeleteProject(int projectId) =>
        ToActionResult(await curriculumService.DeleteProjectAsync(projectId));

    [HttpGet("projects/{projectId:int}/terms")]
    public async Task<ActionResult<ResponseService<List<CurriculumNodeDto>>>> GetTerms(int projectId) =>
        ToActionResult(await curriculumService.GetTermsAsync(projectId));

    [HttpPost("projects/{projectId:int}/terms")]
    public async Task<ActionResult<ResponseService<CurriculumNodeDto>>> CreateTerm(int projectId, [FromBody] CreateTermDto body) =>
        ToActionResult(await curriculumService.CreateTermAsync(projectId, body.Name, body.StartDate, body.EndDate));

    [HttpGet("terms/{termId:int}")]
    public async Task<ActionResult<ResponseService<CurriculumNodeDto>>> GetTerm(int termId) =>
        ToActionResult(await curriculumService.GetTermAsync(termId));

    [HttpPatch("terms/{termId:int}")]
    public async Task<ActionResult<ResponseService<CurriculumNodeDto>>> UpdateTerm(int termId, [FromBody] UpdateTermDto body) =>
        ToActionResult(await curriculumService.UpdateTermAsync(termId, body.Name, body.StartDate, body.EndDate));

    [HttpDelete("terms/{termId:int}")]
    public async Task<ActionResult<ResponseService<bool>>> DeleteTerm(int termId) =>
        ToActionResult(await curriculumService.DeleteTermAsync(termId));

    [HttpGet("terms/{termId:int}/subject-groups")]
    public async Task<ActionResult<ResponseService<List<CurriculumNodeDto>>>> GetSubjectGroups(int termId) =>
        ToActionResult(await curriculumService.GetSubjectGroupsAsync(termId));

    [HttpPost("terms/{termId:int}/subject-groups")]
    public async Task<ActionResult<ResponseService<CurriculumNodeDto>>> CreateSubjectGroup(int termId, [FromBody] CreateSubjectGroupDto body) =>
        ToActionResult(await curriculumService.CreateSubjectGroupAsync(termId, body.Name));

    [HttpGet("subject-groups/{subjectGroupId:int}")]
    public async Task<ActionResult<ResponseService<CurriculumNodeDto>>> GetSubjectGroup(int subjectGroupId) =>
        ToActionResult(await curriculumService.GetSubjectGroupAsync(subjectGroupId));

    [HttpPatch("subject-groups/{subjectGroupId:int}")]
    public async Task<ActionResult<ResponseService<CurriculumNodeDto>>> UpdateSubjectGroup(int subjectGroupId, [FromBody] UpdateSubjectGroupDto body) =>
        ToActionResult(await curriculumService.UpdateSubjectGroupAsync(subjectGroupId, body.Name));

    [HttpDelete("subject-groups/{subjectGroupId:int}")]
    public async Task<ActionResult<ResponseService<bool>>> DeleteSubjectGroup(int subjectGroupId) =>
        ToActionResult(await curriculumService.DeleteSubjectGroupAsync(subjectGroupId));

    [HttpGet("subject-groups/with-subjects")]
    public async Task<ActionResult<ResponseService<List<CurriculumNodeDto>>>> GetSubjectGroupsWithSubjects() =>
        ToActionResult(await curriculumService.GetSubjectGroupNodesWithSubjectsAsync());

    private static ActionResult<ResponseService<T>> ToActionResult<T>(ResponseService<T> r) =>
        r.Error ? new BadRequestObjectResult(r) : new OkObjectResult(r);
}
