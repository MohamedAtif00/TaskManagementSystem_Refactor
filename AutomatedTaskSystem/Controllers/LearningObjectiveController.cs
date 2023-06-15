using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using Microsoft.AspNetCore.Mvc;
using AutomatedTaskSystem.Services.TaskService;

namespace AutomatedTaskSystem.Controllers
{
    [Route("learning-objectives")]
    [ApiController]
    public class LearningObjectiveController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ITaskService _taskService;

        public LearningObjectiveController(DataContext context, ITaskService taskService)
        {
            _taskService = taskService;
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
                .ThenInclude(t => t.Comments)
                .FirstOrDefaultAsync();

            if (lo == null)
                return NotFound(new Responses.BadRequestsDTO("Learning Objective not Found"));

            foreach (var t in lo.Tasks)
            {
                foreach (var c in t.Comments)
                    c.Archived = true;

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
            if (req.Steps.Count == 0)
                return BadRequest(new Responses.BadRequestsDTO("Please supply steps"));

            var lo = await _context.LearningObjectives
                .Where(lo => lo.Id == id)
                .FirstOrDefaultAsync();

            if (lo == null)
                return NotFound(new Responses.BadRequestsDTO("Learning Objective not Found"));

            if (lo.SchemaId != req.SchemaId)
            {
                var schema = await _context.Schemas
                    .Where(s => s.Id == req.SchemaId)
                    .FirstOrDefaultAsync();

                if (schema == null)
                    return NotFound(new Responses.BadRequestsDTO("Schema not Found"));

                lo.Schema = schema;
                lo.SchemaId = schema.Id;

                var tasks = await _context.Tasks
                    .Where(t => t.LearningObjectiveId == lo.Id)
                    .ToListAsync();

                var COSEA = await _context.EndActivityTypes.FindAsync(6);
                if (COSEA != null)
                {
                    var status = await _context.Statuses.FindAsync(4);
                    if (status == null)
                        return NotFound(new Responses.BadRequestsDTO("Done status not Found"));

                    foreach (var task in tasks)
                    {
                        task.Archived = true;
                        task.Status = status;
                        task.StatusId = status.Id;

                        var currentEA = await _context.EndActivities
                            .Where(
                                _ =>
                                    _.UserId == task.UserId
                                    && _.TaskId == task.Id
                                    && _.EndDate == null
                                    && _.EndActivityTypeId == null
                            )
                            .FirstOrDefaultAsync();

                        if (currentEA != null)
                        {
                            currentEA.EndActivityTypeId = COSEA.Id;
                            currentEA.EndActivityType = COSEA;
                            currentEA.EndDate = DateTime.Now;
                        }
                    }
                }

                if (req.Steps.Count < 0)
                    return NotFound(new Responses.BadRequestsDTO("Please supply steps"));

                foreach (var stepId in req.Steps)
                {
                    var step = await _context.Steps
                        .Where(s => s.Id == stepId)
                        .Include(s => s.TaskBank)
                        .ThenInclude(tb => tb.Group)
                        .FirstOrDefaultAsync();

                    if (step == null)
                        return NotFound(new Responses.BadRequestsDTO("Step not Found"));

                    var newTask = await _taskService.CreateTaskWithStep(step, lo);
                }
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
