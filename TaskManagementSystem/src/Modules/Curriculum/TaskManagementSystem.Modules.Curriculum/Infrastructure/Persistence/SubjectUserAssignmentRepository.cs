using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence;

internal sealed class SubjectUserAssignmentRepository(CurriculumDbContext context) : ISubjectUserAssignmentRepository
{
    public async Task<IReadOnlyList<int>> ListUserIdsBySubjectIdAsync(
        int subjectId,
        CancellationToken cancellationToken = default) =>
        await context.SubjectUserAssignments.AsNoTracking()
            .Where(assignment => assignment.SubjectsId == subjectId)
            .Select(assignment => assignment.UsersId)
            .OrderBy(userId => userId)
            .ToListAsync(cancellationToken);

    public async Task AssignUsersAsync(
        int subjectId,
        IReadOnlyCollection<int> userIds,
        CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0)
        {
            return;
        }

        var existingUserIds = await context.SubjectUserAssignments.AsNoTracking()
            .Where(assignment => assignment.SubjectsId == subjectId)
            .Select(assignment => assignment.UsersId)
            .ToListAsync(cancellationToken);

        foreach (var userId in userIds.Distinct().Except(existingUserIds))
        {
            await context.SubjectUserAssignments.AddAsync(
                SubjectUserAssignment.Create(subjectId, userId),
                cancellationToken);
        }
    }

    public async Task UnassignUsersAsync(
        int subjectId,
        IReadOnlyCollection<int> userIds,
        CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0)
        {
            return;
        }

        var assignments = await context.SubjectUserAssignments
            .Where(assignment => assignment.SubjectsId == subjectId && userIds.Contains(assignment.UsersId))
            .ToListAsync(cancellationToken);

        context.SubjectUserAssignments.RemoveRange(assignments);
    }
}
