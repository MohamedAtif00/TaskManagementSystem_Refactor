using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.RootProjectService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Controllers;
[Route("folders")]
[ApiController]
public class RootProjectController(IFolderService folderService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ResponseService<List<FolderDto>>>> GetRoots() =>
        (await folderService.GetRootsAsync()).ToActionResult();

    [HttpGet("{folderId:int}")]
    public async Task<ActionResult<ResponseService<FolderDto>>> Get(int folderId) =>
        (await folderService.GetAsync(folderId)).ToActionResult();

    [HttpPost]
    public async Task<ActionResult<ResponseService<FolderDto>>> Create(
        [FromBody] CreateFolderRequestDto body
    ) => (await folderService.CreateAsync(body.Name, body.ParentFolderId, body.Description, body.LevelNames)).ToActionResult();

    [HttpPatch("{folderId:int}")]
    public async Task<ActionResult<ResponseService<FolderDto>>> Update(
        int folderId,
        [FromBody] UpdateFolderRequestDto body
    ) => (await folderService.UpdateAsync(folderId, body.Name, body.Description, body.LevelNames)).ToActionResult();

    [HttpGet("{folderId:int}/children")]
    public async Task<ActionResult<ResponseService<List<FolderDto>>>> GetChildren(int folderId) =>
        (await folderService.GetChildrenAsync(folderId)).ToActionResult();

    [HttpGet("with-subjects")]
    public async Task<ActionResult<ResponseService<List<FolderDto>>>> GetFoldersWithSubjects() =>
        (await folderService.GetSubjectsFoldersAsync()).ToActionResult();
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
