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
using static AutomatedTaskSystem.DTO.Responses;
using AutomatedTaskSystem.Services.Lesson;
using AutomatedTaskSystem.Dtos.Tasks;
using System.Linq;

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
        private readonly ILessonService _lessonService;

        public LearningObjectiveController(
            DataContext context,
            ITaskService taskService,
            ITokenService tokenService,
            IAuthService authService
,
            ILessonService lessonService)
        {
            _taskService = taskService;
            _tokenService = tokenService;
            _authService = authService;
            _context = context;
            _lessonService = lessonService;
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
            // authinticate user
            var user = await _authService.GetAuthedUser();
			if (user is null || user.Role != UserRoleEnum.ProjectManger)
				return Unauthorized(new BaseResponseService {
						Error = true,
						Message = "Unauthorized"
				});
            //

   
            // get LO
            var lo = await _context.LearningObjectives
                .Where(lo => lo.Id == id)
                .Include(lo => lo.Schema)
                .FirstOrDefaultAsync();

            if (lo == null)
                return NotFound(new Responses.BadRequestsDTO("Learning Objective not Found"));

            //if (req.Steps.Count > 0)
            //    return NotFound(new Responses.BadRequestsDTO("Please supply steps"));


            if (lo.SchemaId != req.SchemaId)
            {
                var schema = await _context.Schemas
                    .Where(s => s.Id == req.SchemaId)
                    .FirstOrDefaultAsync();

                var loTasks = await _context.Tasks
                    .Include(x => x.LearningObjective)
                    .Where(t => t.LearningObjectiveId == lo.Id && !t.Archived)
                    .ToListAsync();


                /////////////////////////
                //Create new instance from old LO
                //var new

                var newLo = await _lessonService.CreateLO(lo.LessonId,
                         new Requests.LearningObjectiveDTO
                         {
                             
                             Name = req.Name,
                             Tag = req.Tag,
                             Template = req.Template,
                             Environment = req.Environment,
                             SchemaId = req.SchemaId
                         });

                // List of steps we want to skip (stop points)
                List<int> stopStepIds = req.Steps;

                // Get ordered nodes with their steps
                var orderedNodes = await _context.Nodes
                    .Where(n => n.SchemaId == schema.Id)
                    .OrderBy(n => n.Order)
                    .Include(n => n.Steps)
                    .ToListAsync();

                // Function to fetch all active tasks (excluding stopStepIds)
                async Task<List<Models.Task>> GetActiveTasksAsync() =>
                    await _context.Tasks
                        .Where(x => x.LearningObjectiveId == newLo.Id &&
                                    x.Status != TaskStatusEnum.Done &&
                                    !x.Archived &&
                                    x.StepId != null &&
                                    !stopStepIds.Contains(x.StepId.Value))
                        .Include(x => x.Step)
                        .ToListAsync();

                // Initial fetch of active tasks
                var activeTasks = await GetActiveTasksAsync();

                // Loop while there are still active tasks not in the stopStepIds
                while (activeTasks.Any())
                {
                    foreach (var node in orderedNodes)
                    {
                        foreach (var step in node.Steps.OrderBy(s => s.Order))
                        {
                            // Skip steps we don’t want to complete now
                            if (stopStepIds.Contains(step.Id))
                                continue;

                            var tasksToComplete = activeTasks
                                .Where(t => t.StepId == step.Id)
                                .ToList();

                            foreach (var task in tasksToComplete)
                            {
                                var response = await _taskService.CompleteTask(task.Id, true);
                                GetTaskDetailsDto taskDetails = response.Value.Data;
                            }

                            // Refresh active tasks after potential new tasks were generated
                            activeTasks = await GetActiveTasksAsync();

                            // If no remaining tasks, break early
                            if (!activeTasks.Any())
                                break;
                        }

                        // If no remaining tasks, break outer loop
                        if (!activeTasks.Any())
                            break;
                    }

                    // Final refresh to check for any new tasks again
                    activeTasks = await GetActiveTasksAsync();
                }



                var lesson = await _context.Lessons.FirstOrDefaultAsync(x => x.Id == lo.LessonId);


                if (schema is null)
                    return NotFound(new Responses.BadRequestsDTO("Schema not found"));




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
                    //task.Name = req.Name;

                    task.Status = TaskStatusEnum.Done;

                    //task.Archived = true;
                }

                //var res = await _taskService.CreateProcess(
                //    options: req.Steps,
                //    schemaId: req.SchemaId,
                //    loId: lo.Id
                //);

                //if (res.Error)
                //    return BadRequest(res);

                //lo.Schema = schema;
                //lo.SchemaId = schema.Id;
            }
            else { 
                lo.Environment = req.Environment;
                lo.Template = req.Template;
                lo.Tag = req.Tag;
                lo.Name = req.Name;
            
            }
            if (lo.SchemaId != req.SchemaId)
                lo.Name = lo.Name + "_old_" + DateTime.Now.Day + "_" + DateTime.Now.Month;
            else
            { 
                lo.Name = req.Name;
                //lo.DoneAt = DateTime.Now;            
            }
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
