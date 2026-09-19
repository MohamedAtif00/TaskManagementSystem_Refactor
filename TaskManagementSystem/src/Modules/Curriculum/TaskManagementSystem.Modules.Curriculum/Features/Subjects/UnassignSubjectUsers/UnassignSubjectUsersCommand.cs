using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.UnassignSubjectUsers;

public sealed record UnassignSubjectUsersCommand(int SubjectId, IReadOnlyList<int> UserIds) : ICommand<Result<NoValue>>;

