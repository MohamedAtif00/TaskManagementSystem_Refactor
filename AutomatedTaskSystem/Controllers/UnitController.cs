using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Controllers
{
	[Route("units")]
	[ApiController]
	public class UnitController : ControllerBase
	{
		private readonly DataContext _context;
		public UnitController(DataContext context)
		{
			_context = context;
		}
		// POST:
		// Add a lesson for a unit
		[HttpPost("{id}/lessons")]
		public async Task<ActionResult<Responses.ProjectLessonDTO>> AddLesson(int id, Requests.NameDTO req)
		{
			var unit = await _context.Units
				.Where(u => u.Id == id)
				.FirstOrDefaultAsync();

			if (unit == null)
			{
				return NotFound(new Responses.BadRequestsDTO("Unit not found"));
			}

			var newLesson = new Lesson
			{
				Unit = unit,
				UnitId = unit.Id,
				Name = req.Name
			};

			_context.Lessons.Add(newLesson);
			unit.Lessons.Add(newLesson);

			await _context.SaveChangesAsync();

			return Ok(new Responses.ProjectLessonDTO { Name = newLesson.Name, Id = newLesson.Id });
		}
		// DELETE:
		// Remove a unit with all models under it
		[HttpDelete("{id}")]
		public async Task<ActionResult<Responses.SuccessDTO>> DeleteUnit(int id)
		{
			var unit = await _context.Units
				.Where(u => u.Id == id)
				.Include(u => u.Lessons)
				.FirstOrDefaultAsync();

			if (unit == null)
				return NotFound(new Responses.BadRequestsDTO("Unit not found"));

			for (int i = 0; i < unit.Lessons.Count; i++)
			{
				var lesson = unit.Lessons[i];
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
			}

			_context.Units.Remove(unit);
			await _context.SaveChangesAsync();

			return Ok(new Responses.SuccessDTO("Unit Deleted"));
		}
	}
}