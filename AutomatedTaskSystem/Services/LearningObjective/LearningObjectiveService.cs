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

    private static bool IsOldLearningObjectiveName(string? name) =>
        !string.IsNullOrWhiteSpace(name) && name.Contains("old", StringComparison.OrdinalIgnoreCase);

    public async Task<ResponseService<List<LearningObjective>>> GetLearningObjectivesBySubjectId(
        int subjectId
    )
    {
        var subject = await _context.Subjects
            .Where(p => !p.Archived && p.Id == subjectId)
            .Include(p => p.Units)
            .ThenInclude(u => u.Lessons)
            .ThenInclude(l => l.LearningObjectives.Where(x => x.DoneAt == null && !x.Archived))
            .FirstOrDefaultAsync();

        if (subject is null)
            return new ResponseService<List<LearningObjective>>
            {
                Error = true,
                Message = "Subject is not found"
            };

        var los = new List<LearningObjective>();

        foreach (var unit in subject.Units)
            if (!unit.Archived)
                foreach (var lesson in unit.Lessons)
                    if (!lesson.Archived)
                        foreach (var lo in lesson.LearningObjectives)
                            if (!lo.Archived && !IsOldLearningObjectiveName(lo.Name))
                                los.Add(lo);

        return new ResponseService<List<LearningObjective>>
        {
            Data = los,
            Error = false,
            Message = $"List of learning objectives in subject of id:{subject.Id}"
        };
    }
}
