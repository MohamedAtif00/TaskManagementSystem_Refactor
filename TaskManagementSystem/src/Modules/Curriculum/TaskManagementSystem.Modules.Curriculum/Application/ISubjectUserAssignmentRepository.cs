namespace TaskManagementSystem.Modules.Curriculum.Application;

public interface ISubjectUserAssignmentRepository
{
    Task<IReadOnlyList<int>> ListUserIdsBySubjectIdAsync(int subjectId, CancellationToken cancellationToken = default);

    Task AssignUsersAsync(int subjectId, IReadOnlyCollection<int> userIds, CancellationToken cancellationToken = default);

    Task UnassignUsersAsync(int subjectId, IReadOnlyCollection<int> userIds, CancellationToken cancellationToken = default);
}
