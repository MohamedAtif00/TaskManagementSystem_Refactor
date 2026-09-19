using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Lessons.GetLessonById;

public sealed record GetLessonByIdQuery(int Id) : IQuery<Result<LessonDetailResult>>;

