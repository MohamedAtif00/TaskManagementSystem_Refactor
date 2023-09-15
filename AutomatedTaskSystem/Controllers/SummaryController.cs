using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Controllers
{
    [Route("projects")]
    [ApiController]
    public class SummaryController : ControllerBase
    {
        private readonly DataContext _context;

        public SummaryController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("{id}/summary")]
        public async Task<ActionResult<Responses.ReportDTO>> GetSummary(int id)
        {
            var project = await _context.Projects
                .Where(p => p.Id == id && !p.Archived)
                .Include(p => p.Units)
                .ThenInclude(u => u.Lessons)
                .ThenInclude(l => l.LearningObjectives)
                .ThenInclude(lo => lo.Schema)
                .Include(p => p.Units)
                .ThenInclude(u => u.Lessons)
                .ThenInclude(l => l.LearningObjectives)
                .ThenInclude(lo => lo.Tasks)
                .FirstOrDefaultAsync();

            if (project == null)
                return NotFound(new Responses.BadRequestsDTO("Project not found"));

            var response = new Responses.ReportDTO
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
            };
            foreach (var u in project.Units)
            {
                if (u.Archived)
                    continue;
                var _unit = new Responses.Unit { Name = u.Name, Id = u.Id, };
                foreach (var l in u.Lessons)
                {
                    if (l.Archived)
                        continue;
                    var _lesson = new Responses.Lesson { Id = l.Id, Name = l.Name, };
                    foreach (var lo in l.LearningObjectives)
                    {
                        if (lo.Archived)
                            continue;
                        var _lo = new Responses.LearningObjective
                        {
                            Name = lo.Name,
                            Id = lo.Id,
                            Tag = lo.Tag,
                            Schema = new Responses.IDName
                            {
                                Name = lo.Schema.Name,
                                Id = lo.Schema.Id
                            },
                            Template = lo.Template,
                            Environment = lo.Environment
                        };
                        foreach (var task in lo.Tasks)
                        {
                            if (task.Archived)
                                continue;
                            var _task = new Responses.Task
                            {
                                Id = task.Id,
                                Name = task.Name,
                                Status = task.Status
                            };
                            _lo.Tasks.Add(_task);
                        }
                        _lesson.LearningObjectives.Add(_lo);
                    }
                    _unit.Lessons.Add(_lesson);
                }
                response.Units.Add(_unit);
            }

            return response;
        }
    }
}
