using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;

namespace AutomatedTaskSystem.Services.LearningObjectiveService;

public class LearningObjectiveService : ILearningObjectiveService
{
	private readonly DataContext _context;

	public LearningObjectiveService(DataContext context)
	{
		_context = context;
	}
	public async Task<ResponseService<List<LearningObjective>>> GetLearningObjectivesByProjectId(int Pid)
	{
		var project = await _context.Projects
			.Where(p => !p.Archived && p.Id == Pid)
			.Include(p => p.Units)
			.ThenInclude(u => u.Lessons)
			.ThenInclude(l => l.LearningObjectives)
			.FirstOrDefaultAsync();

		if (project is null)
			return new ResponseService<List<LearningObjective>>
			{
				Error = true,
				Message = "Project is not found"
			};

		var los = new List<LearningObjective> { };

		foreach (var unit in project.Units)
			foreach (var lesson in unit.Lessons)
				foreach (var lo in lesson.LearningObjectives)
					los.Add(lo);


		return new ResponseService<List<LearningObjective>>
		{
			Data = los,
			Error = false,
			Message = $"List of learning objectives in project of id:{project.Id}"
		};

		throw new NotImplementedException();
	}
}