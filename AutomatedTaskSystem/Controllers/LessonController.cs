using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using Microsoft.AspNetCore.Mvc;
using AutomatedTaskSystem.Services.PathService;
using AutomatedTaskSystem.Services.TaskService;

namespace AutomatedTaskSystem.Controllers
{
    [Route("lessons")]
    [ApiController]
    public class LessonController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IPathService _pathService;
        private readonly ITaskService _taskService;

        public LessonController(
            DataContext context,
            IPathService pathService,
            ITaskService taskService
        )
        {
            _context = context;
            _pathService = pathService;
            _taskService = taskService;
        }

        // POST:
        // Add LO to lesson
        [HttpPost("{id}/learning-objective")]
        public async Task<ActionResult<Responses.LearningObjectiveDTO>> AddLO(
            int id,
            Requests.LearningObjectiveDTO req
        )
        {
            var lesson = await _context.Lessons.Where(l => l.Id == id).FirstOrDefaultAsync();

            if (lesson == null)
                return NotFound(new Responses.BadRequestsDTO("Lesson not found"));

            var schema = await _context.Schemas
                .Where(s => s.Id == req.SchemaId)
                .Include(s => s.Nodes)
                .ThenInclude(n => n.Steps)
                .ThenInclude(s => s.TaskBank)
                .ThenInclude(tb => tb.Group)
                .Include(s => s.Nodes)
                .ThenInclude(n => n.Next)
                .FirstOrDefaultAsync();

            if (schema == null)
                return NotFound(new Responses.BadRequestsDTO("Schema not found"));

            var newLO = new LearningObjective
            {
                Environment = req.Environment,
                Lesson = lesson,
                LessonId = lesson.Id,
                Name = req.Name,
                Schema = schema,
                SchemaId = schema.Id,
                Tag = req.Tag,
                Template = req.Template,
                Tasks = new List<Models.Task> { },
                Archived = false
            };

            schema.LearningObjectives.Add(newLO);
            lesson.LearningObjectives.Add(newLO);
            _context.LearningObjectives.Add(newLO);

			await _context.SaveChangesAsync();

            await _pathService.GeneratePath(schema.Id, newLO);

            var firstNodes = schema.Nodes.Where(n => n.isStart).ToList();

            foreach (var node in firstNodes)
            {
                var firstStep = node.Steps.Where(s => s.Order == 1).FirstOrDefault();

                if (firstStep is not null)
                {
                    var newTask = await _taskService.CreateTaskWithStep(firstStep, newLO);

					await _pathService.UpdatePathTask(newTask, firstStep);
                }
            }

            return Ok(
                new Responses.LearningObjectiveDTO
                {
                    Id = newLO.Id,
                    Schema = new Responses.IDName { Id = schema.Id, Name = schema.Name },
                    Name = newLO.Name,
                    Tag = newLO.Tag,
                    Template = newLO.Template,
                    Environment = newLO.Environment
                }
            );
        }

        // DELETE:
        // Delete a lessons with all is sub models
        [HttpDelete("{id}")]
        public async Task<ActionResult<Responses.SuccessDTO>> DeleteLesson(int id)
        {
            var lesson = await _context.Lessons.Where(l => l.Id == id).FirstOrDefaultAsync();

            if (lesson == null)
                return NotFound(new Responses.BadRequestsDTO("Lesson not found"));

            var los = await _context.LearningObjectives
                .Where(lo => lo.LessonId == lesson.Id)
                .Include(lo => lo.Tasks)
                .ToListAsync();

            los.ForEach(lo =>
            {
                _context.LearningObjectives.Remove(lo);
                lo.Tasks.ForEach(t =>
                {
                    _context.Tasks.Remove(t);
                });
            });
            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            return Ok(new Responses.SuccessDTO("Lesson Deleted"));
        }
    }
}
