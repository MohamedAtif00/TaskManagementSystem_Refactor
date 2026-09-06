using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums.TaskStatus;
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
            .ThenInclude(l => l.LearningObjectives.Where(x => !x.Archived))
            .FirstOrDefaultAsync();

        if (subject is null)
            return new ResponseService<List<LearningObjective>>
            {
                Error = true,
                Message = "Subject is not found"
            };

        var candidateLos = subject.Units
            .Where(unit => !unit.Archived)
            .SelectMany(unit => unit.Lessons)
            .Where(lesson => !lesson.Archived)
            .SelectMany(lesson => lesson.LearningObjectives)
            .Where(lo => !lo.Archived && !IsOldLearningObjectiveName(lo.Name))
            .ToList();

        var staleCompletedLoIds = candidateLos
            .Where(lo => lo.DoneAt != null)
            .Select(lo => lo.Id)
            .ToList();

        if (staleCompletedLoIds.Count > 0)
        {
            var remainingLoIds = (await _context.Tasks
                .Where(t =>
                    !t.Archived
                    && staleCompletedLoIds.Contains(t.LearningObjectiveId)
                    && t.Status != TaskStatusEnum.Done)
                .Select(t => t.LearningObjectiveId)
                .Distinct()
                .ToListAsync())
                .ToHashSet();

            foreach (var lo in candidateLos.Where(lo => remainingLoIds.Contains(lo.Id)))
                lo.DoneAt = null;

            if (remainingLoIds.Count > 0)
                await _context.SaveChangesAsync();
        }

        var los = candidateLos.Where(lo => lo.DoneAt == null).ToList();

        return new ResponseService<List<LearningObjective>>
        {
            Data = los,
            Error = false,
            Message = $"List of learning objectives in subject of id:{subject.Id}"
        };
    }
}
