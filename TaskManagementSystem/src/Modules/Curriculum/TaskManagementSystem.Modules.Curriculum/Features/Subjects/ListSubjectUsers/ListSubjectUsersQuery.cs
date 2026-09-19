using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.ListSubjectUsers;

public sealed record ListSubjectUsersQuery(int SubjectId) : IQuery<Result<IReadOnlyList<SubjectUserResult>>>;

