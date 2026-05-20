using AutomatedTaskSystem.DTO;

using AutomatedTaskSystem.Services.ResponseService;

using AutomatedTaskSystem.Services.RootProjectService;

using Microsoft.AspNetCore.Mvc;



namespace AutomatedTaskSystem.Controllers;



[Route("projects")]

[ApiController]

public class RootProjectController(IRootProjectService rootProjectService) : ControllerBase

{

    [HttpGet]

    public async Task<ActionResult<ResponseService<List<RootProjectListDto>>>> GetAll() =>

        (await rootProjectService.GetAllAsync()).ToActionResult();



    [HttpGet("{rootProjectId:int}")]

    public async Task<ActionResult<ResponseService<RootProjectDetailDto>>> Get(int rootProjectId) =>

        (await rootProjectService.GetAsync(rootProjectId)).ToActionResult();



    [HttpPost]

    public async Task<ActionResult<ResponseService<RootProjectListDto>>> Create(

        [FromBody] CreateRootProjectRequest body

    ) => (await rootProjectService.CreateAsync(body.Name, body.Description)).ToActionResult();



    [HttpPatch("{rootProjectId:int}")]

    public async Task<ActionResult<ResponseService<RootProjectDetailDto>>> Update(

        int rootProjectId,

        [FromBody] CreateRootProjectRequest body

    ) => (await rootProjectService.UpdateAsync(rootProjectId, body.Name, body.Description)).ToActionResult();



    [HttpGet("{rootProjectId:int}/years")]

    public async Task<ActionResult<ResponseService<List<Responses.IDName>>>> GetYears(int rootProjectId) =>

        (await rootProjectService.GetYearsAsync(rootProjectId)).ToActionResult();



    [HttpPost("{rootProjectId:int}/years")]

    public async Task<ActionResult<ResponseService<Responses.IDName>>> CreateYear(

        int rootProjectId,

        [FromBody] LabelRequest body

    ) => (await rootProjectService.CreateYearAsync(rootProjectId, body.Label)).ToActionResult();



    [HttpGet("years/{projectYearId:int}")]

    public async Task<ActionResult<ResponseService<ProjectYearDetailDto>>> GetYear(int projectYearId) =>

        (await rootProjectService.GetYearAsync(projectYearId)).ToActionResult();



    [HttpPatch("years/{projectYearId:int}")]

    public async Task<ActionResult<ResponseService<ProjectYearDetailDto>>> UpdateYear(

        int projectYearId,

        [FromBody] LabelRequest body

    ) => (await rootProjectService.UpdateYearAsync(projectYearId, body.Label)).ToActionResult();



    [HttpGet("years/{projectYearId:int}/terms")]

    public async Task<ActionResult<ResponseService<List<ProjectTermListDto>>>> GetTerms(int projectYearId) =>

        (await rootProjectService.GetTermsAsync(projectYearId)).ToActionResult();



    [HttpPost("years/{projectYearId:int}/terms")]

    public async Task<ActionResult<ResponseService<ProjectTermListDto>>> CreateTerm(

        int projectYearId,

        [FromBody] CreateTermRequest body

    ) =>

        (

            await rootProjectService.CreateTermAsync(

                projectYearId,

                body.Name,

                body.Order,

                body.StartDate,

                body.EndDate

            )

        ).ToActionResult();



    [HttpGet("terms/{termId:int}")]

    public async Task<ActionResult<ResponseService<ProjectTermDetailDto>>> GetTerm(int termId) =>

        (await rootProjectService.GetTermAsync(termId)).ToActionResult();



    [HttpPatch("terms/{termId:int}")]

    public async Task<ActionResult<ResponseService<ProjectTermDetailDto>>> UpdateTerm(

        int termId,

        [FromBody] UpdateTermRequest body

    ) =>

        (

            await rootProjectService.UpdateTermAsync(

                termId,

                body.Name,

                body.Order,

                body.StartDate,

                body.EndDate

            )

        ).ToActionResult();

}



public class CreateRootProjectRequest

{

    public string Name { get; set; } = "";

    public string? Description { get; set; }

}



public class LabelRequest

{

    public string Label { get; set; } = "";

}



public class CreateTermRequest

{

    public string Name { get; set; } = "";

    public int Order { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

}



public class UpdateTermRequest

{

    public string Name { get; set; } = "";

    public int Order { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

}



internal static class ResponseServiceExtensions

{

    public static ActionResult<ResponseService<T>> ToActionResult<T>(this ResponseService<T> r)

    {

        if (r.Error)

            return new BadRequestObjectResult(r);

        return new OkObjectResult(r);

    }

}


