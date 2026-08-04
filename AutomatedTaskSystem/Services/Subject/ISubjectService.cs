using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models.Enums.ProjectStatus;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;
using static AutomatedTaskSystem.DTO.Responses;

namespace AutomatedTaskSystem.Services.SubjectService;

public interface ISubjectService
{
    Task<ActionResult<ResponseService<ProjectUnitDTO>>> AddUnit(int Id, string Name);
    Task<ActionResult<BaseResponseService>> DeleteProject(int Id);
    Task<ActionResult<ResponseService<List<IDName>>>> AssignToProject(
        int Id,
        List<int> UserIds
    );
    Task<ActionResult<ResponseService<SubjectDTO>>> CreateProject(
        string Name,
        string Description,
        int folderId
    );
    Task<ActionResult<ResponseService<SubjectDTO>>> CopyProject(
        int sourceId,
        Requests.CopySubjectRequest req
    );
    Task<ActionResult<ResponseService<SubjectDTO>>> EditProject(
        int id,
        string Name,
        string Description,
        int folderId
    );
    Task<ActionResult<ResponseService<List<UserDTO>>>> GetUnassignedUsers(int Id);
    Task<ActionResult<ResponseService<List<UserDTO>>>> GetAssignedUsers(int Id);
    Task<ActionResult<ResponseService<List<SubjectDTO>>>> GetUserSpecificProjects();
    Task<ActionResult<ResponseService<List<IDName>>>> UnassignToProject(
        int Id,
        List<int> UserIds
    );
    Task<ActionResult<ResponseService<List<SubjectDTO>>>> GetAllProjects();
    Task<ActionResult<ResponseService<List<SubjectDTO>>>> GetSubjectsByFolder(
        int folderId,
        bool includeInactiveStatuses = false
    );
    Task<ActionResult<ResponseService<List<SubjectCopyLineageChainDTO>>>> GetCopyLineagesByFolder(
        int folderId
    );
    Task<ActionResult<ResponseService<List<IDName>>>> GetProjectLearningObjectives(
        int Id
    );
    Task<ActionResult<ResponseService<SubjectDTO>>> GetProject(int Id);
    Task<ActionResult<ResponseService<DetailedProjectDTO>>> GetProjectDetails(int Id);
    Task<ActionResult<ResponseService<SubjectDTO>>> UpdateProjectStatus(
        int id,
        ProjectStatusEnum priority
    );
    Task<ActionResult<ResponseService<List<SubjectDTO>>>> GetAllProjectsForSprint();
}
