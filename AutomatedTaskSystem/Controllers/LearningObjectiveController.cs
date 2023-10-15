using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using Microsoft.AspNetCore.Mvc;
using AutomatedTaskSystem.Services.TaskService;
using AutomatedTaskSystem.Services.TokenService;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums.TaskActivityType;
using AutomatedTaskSystem.Services.AuthService;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Models.Enums.UserRole;
using AutomatedTaskSystem.Models.Enums.TaskStatus;

namespace AutomatedTaskSystem.Controllers
{
    [Route("learning-objectives")]
    [ApiController]
    public class LearningObjectiveController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ITaskService _taskService;
        private readonly ITokenService _tokenService;
        private readonly IAuthService _authService;

        public LearningObjectiveController(
            DataContext context,
            ITaskService taskService,
            ITokenService tokenService,
            IAuthService authService
        )
        {
            _taskService = taskService;
            _tokenService = tokenService;
            _authService = authService;
            _context = context;
        }

        // DELETE:
        // Delete a lo with all is sub models
        [HttpDelete("{id}")]
        public async Task<ActionResult<Responses.SuccessDTO>> DeleteLearningObjective(int id)
        {
            var lo = await _context.LearningObjectives
                .Where(lo => lo.Id == id)
                .Include(lo => lo.Tasks)
                .Include(lo => lo.Comments)
                .FirstOrDefaultAsync();

            if (lo == null)
                return NotFound(new Responses.BadRequestsDTO("Learning Objective not Found"));

            foreach (var c in lo.Comments)
                c.Archived = true;

            foreach (var t in lo.Tasks)
            {
                t.Archived = true;
            }

            lo.Archived = true;
            await _context.SaveChangesAsync();

            return Ok(new Responses.SuccessDTO("Learning Objective Deleted"));
        }

        // PATCH:
        // Edit a lo all is sub models
        [HttpPatch("{id}")]
        public async Task<ActionResult<Responses.LearningObjectiveDTO>> EditLearningObjective(
            int id,
            Requests.EditLearningObjectiveDTO req
        )
        {
            var user = await _authService.GetAuthedUser();
			if (user is null || user.Role != UserRoleEnum.ProjectManger)
				return Unauthorized(new BaseResponseService {
						Error = true,
						Message = "Unauthorized"
				});

            var lo = await _context.LearningObjectives
                .Where(lo => lo.Id == id)
                .Include(lo => lo.Schema)
                .FirstOrDefaultAsync();

            if (lo == null)
                return NotFound(new Responses.BadRequestsDTO("Learning Objective not Found"));

            if (req.Steps.Count < 0)
                return NotFound(new Responses.BadRequestsDTO("Please supply steps"));

            if (lo.SchemaId != req.SchemaId)
            {
                var schema = await _context.Schemas
                    .Where(s => s.Id == req.SchemaId)
                    .FirstOrDefaultAsync();

                if (schema is null)
                    return NotFound(new Responses.BadRequestsDTO("Schema not found"));

                var loTasks = await _context.Tasks
                    .Where(t => t.LearningObjectiveId == lo.Id && !t.Archived)
                    .ToListAsync();

                foreach (var task in loTasks)
                {
                    var newTaskAct2 = new TaskActivity
                    {
                        Task = task,
                        TaskId = task.Id,
                        Type = TaskActivityTypeEnum.ProcessChange,
                        TimeStamp = DateTime.Now,
                        ActorOne = user,
                        ActorOneId = user.Id,
                        ActorTwo = null,
                        ActorTwoId = null,
                        TaskSecondary = null,
                        TaskSecondaryId = null,
                        AdditionalInfo = null
                    };
                    _context.TaskActivities.Add(newTaskAct2);

					task.Status = TaskStatusEnum.Done;
					task.Archived = true;
                }

                var res = await _taskService.CreateProcess(
                    options: req.Steps,
                    schemaId: req.SchemaId,
                    loId: lo.Id
                );

                if (res.Error)
                    return BadRequest(res);

                lo.Schema = schema;
                lo.SchemaId = schema.Id;
            }
            lo.Environment = req.Environment;
            lo.Template = req.Template;
            lo.Tag = req.Tag;
            lo.Name = req.Name;

            await _context.SaveChangesAsync();

            return new Responses.LearningObjectiveDTO
            {
                Id = lo.Id,
                Name = lo.Name,
                Schema = new Responses.IDName { Name = lo.Schema.Name, Id = lo.Schema.Id },
                Tag = lo.Tag,
                Template = lo.Template,
                Environment = lo.Environment
            };
        }
    }
}
