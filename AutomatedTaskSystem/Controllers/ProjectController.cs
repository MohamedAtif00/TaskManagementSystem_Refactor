using AutomatedTaskSystem.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutomatedTaskSystem.Services.AuthService;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.ProjectService;
using AutomatedTaskSystem.Services.YearService;

namespace AutomatedTaskSystem.Controllers
{
    [Route("projects")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IProjectService _projectService;
        private readonly IYearService _yearService;

        public ProjectController(
            IAuthService authService,
            IProjectService projectService,
            IYearService yearService
        )
        {
            _authService = authService;
            _projectService = projectService;
            _yearService = yearService;
        }

        // Get all unassigned users
        [HttpGet("{id}/users/unassigned")]
        public async Task<
            ActionResult<ResponseService<List<Responses.UserDTO>>>
        > GetUnassignedUsers(int id) => await _projectService.GetUnassignedUsers(id);

        // Get all assigned users
        [HttpGet("{id}/users/assigned")]
        public async Task<ActionResult<ResponseService<List<Responses.UserDTO>>>> GetAssignedUsers(
            int id
        ) => await _projectService.GetAssignedUsers(id);

        // Get user specific assignment
        [Authorize]
        [HttpGet("assignment")]
        public async Task<
            ActionResult<ResponseService<List<Responses.ProjectDTO>>>
        > GetAssignedProject() => await _projectService.GetUserSpecificProjects();

        // Assign to project
        [HttpPost("{id}/assign")]
        public async Task<ActionResult<ResponseService<List<Responses.IDName>>>> AssignToProject(
            int id,
            Requests.LOAssignDTO req
        ) => await _projectService.AssignToProject(id, req.UserIds);

        // Delete from Learning Objective
        [HttpDelete("{id}")]
        public async Task<ActionResult<BaseResponseService>> DeleteProject(int id) =>
            await _projectService.DeleteProject(id);

        // Unassign from project
        [HttpPost("{id}/unassign")]
        public async Task<
            ActionResult<ResponseService<List<Responses.IDName>>>
        > UnassignFromProject(int id, Requests.LOAssignDTO req) =>
            await _projectService.UnassignToProject(id, req.UserIds);

        // Get Project details
        [HttpGet("{id}/details")]
        public async Task<
            ActionResult<ResponseService<Responses.DetailedProjectDTO>>
        > GetProjectDetail(int id) => await _projectService.GetProjectDetails(id);

        // Get Project
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseService<Responses.ProjectDTO>>> GetProject(int id) =>
            await _projectService.GetProject(id);

        // Get all Projects
        [HttpGet]
        public async Task<
            ActionResult<ResponseService<List<Responses.ProjectDTO>>>
        > GetProjects() => await _projectService.GetAllProjects();

        // Get all Projects LO
        [HttpGet("{id}/los")]
        public async Task<ActionResult<ResponseService<List<Responses.IDName>>>> GetProjectsLo(
            int id
        ) => await _projectService.GetProjectLearningObjectives(id);

        // PATCH:
        // Edit project
        [HttpPatch("{id}")]
        public async Task<ActionResult<ResponseService<Responses.ProjectDTO>>> EditProject(
            int id,
            Requests.ProjectDTO req
        ) => await _projectService.EditProject(id, req.Name, req.Description);

        // Create Project
        [HttpPost]
        public async Task<ActionResult<ResponseService<Responses.ProjectDTO>>> CreateProject(
            Requests.ProjectDTO req
        ) => await _projectService.CreateProject(req.Name, req.Description, req.YearId, req.Term);

        // Add unit to project
        [HttpPost("{id}/units")]
        public async Task<ActionResult<ResponseService<Responses.ProjectUnitDTO>>> AddUnit(
            int id,
            Requests.NameDTO req
        ) => await _projectService.AddUnit(id, req.Name);

        // GET:
        // Get Project Years
        [HttpGet("years")]
        public async Task<ActionResult<ResponseService<List<Responses.IDName>>>> GetActiveYears() =>
            await _yearService.GetActiveYears();
    }
}
