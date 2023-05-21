using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.ProjectService;

public interface IProjectService
{
	Task<ActionResult<ResponseService<List<Responses.UserDTO>>>> GetUnassignedUsers(int Id);
	Task<ActionResult<ResponseService<List<Responses.UserDTO>>>> GetAssignedUsers(int Id);
	Task<ActionResult<ResponseService<List<Responses.ProjectDTO>>>> GetUserSpecificProjects();
	Task<ActionResult<ResponseService<List<Responses.IDName>>>> AssignToProject(int Id, List<int> UserIds);
	Task<ActionResult<ResponseService<List<Responses.IDName>>>> UnassignToProject(int Id, List<int> UserIds);
	Task<ActionResult<ResponseService<List<Responses.ProjectDTO>>>> GetAllProjects();
	Task<ActionResult<ResponseService<List<Responses.IDName>>>> GetProjectLearningObjectives(int Id);
	Task<ActionResult<ResponseService<Responses.ProjectDTO>>> CreateProject(string Name, string Description);
	Task<ActionResult<ResponseService<Responses.ProjectDTO>>> GetProject(int Id);
	Task<ActionResult<ResponseService<Responses.DetailedProjectDTO>>> GetProjectDetails(int Id);
	Task<ActionResult<ResponseService<Responses.ProjectUnitDTO>>> AddUnit(int Id, string Name);
}