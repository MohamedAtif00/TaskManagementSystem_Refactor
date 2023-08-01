using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using Microsoft.AspNetCore.Mvc;
using AutomatedTaskSystem.Services.TaskService;
using AutomatedTaskSystem.Services.ResponseService;

namespace AutomatedTaskSystem.Controllers
{
    [Route("lessons")]
    [ApiController]
    public class LessonController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ITaskService _taskService;

        public LessonController(DataContext context, ITaskService taskService)
        {
            _context = context;
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

            var firstNodes = schema.Nodes.Where(n => n.isStart && !n.Archived).ToList();

            foreach (var node in firstNodes)
            {
                var firstStep = node.Steps.Where(s => s.Order == 1 && !s.Archived).FirstOrDefault();

                if (firstStep is not null)
                    await _taskService.CreateTask(firstStep, newLO);
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

        // PATCH:
        // Edit Unit
        [HttpPatch("{id}")]
        public async Task<ActionResult<ResponseService<Responses.IDName>>> EditLesson(
            int id,
            Requests.NameDTO req
        )
        {
            var lesson = await _context.Lessons.Where(u => u.Id == id).FirstOrDefaultAsync();
            if (lesson == null)
                return NotFound(
                    new BaseResponseService { Message = "Lesson is not found", Error = true }
                );

            lesson.Name = req.Name;

            await _context.SaveChangesAsync();

            return new ResponseService<Responses.IDName>
            {
                Data = new Responses.IDName { Name = lesson.Name, Id = lesson.Id },
                Error = false,
                Message = "Lesson updated"
            };
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
                lo.Archived = true;
                lo.Tasks.ForEach(t =>
                {
                    t.Archived = true;
                });
            });
            lesson.Archived = true;
            await _context.SaveChangesAsync();

            return Ok(new Responses.SuccessDTO("Lesson Deleted"));
        }
    }
}
