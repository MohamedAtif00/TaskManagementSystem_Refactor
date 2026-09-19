using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Curriculum.Features.YearTree.GetYearTree;

public sealed record GetYearTreeQuery(int YearId) : IQuery<Result<YearTreeResult>>;

