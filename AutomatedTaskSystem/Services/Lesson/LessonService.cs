using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.TaskService;


namespace AutomatedTaskSystem.Services.Lesson
{
    public class LessonService : ILessonService
    {
        private readonly DataContext _context;
        private readonly ITaskService _taskService;

        public LessonService(DataContext context, ITaskService taskService)
        {
            _context = context;
            _taskService = taskService;
        }


        public async Task<Models.LearningObjective> CreateLO(
            int lessonId,
            Requests.LearningObjectiveDTO req)
        {
            var lesson = await _context.Lessons.Where(l => l.Id == lessonId).FirstOrDefaultAsync();

            //if (lesson == null)
            //    return NotFound(new Responses.BadRequestsDTO("Lesson not found"));

            var schema = await _context.Schemas
                .Where(s => s.Id == req.SchemaId)
                .Include(s => s.Nodes)
                .ThenInclude(n => n.Steps)
                .ThenInclude(s => s.TaskBank)
                .ThenInclude(tb => tb.Group)
                .Include(s => s.Nodes)
                .ThenInclude(n => n.Next)
                .FirstOrDefaultAsync();

            //if (schema == null)
            //    return NotFound(new Responses.BadRequestsDTO("Schema not found"));

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

            return newLO;
        }
    }


}
