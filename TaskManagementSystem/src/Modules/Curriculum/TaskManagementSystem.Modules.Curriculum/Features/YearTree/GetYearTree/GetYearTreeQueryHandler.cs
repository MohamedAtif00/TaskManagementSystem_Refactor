using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Curriculum.Features.YearTree.GetYearTree;

public sealed class GetYearTreeQueryHandler(YearTreeQueries yearTreeQueries)
    : IRequestHandler<GetYearTreeQuery, Result<YearTreeResult>>
{
    public async Task<Result<YearTreeResult>> Handle(GetYearTreeQuery request, CancellationToken cancellationToken)
    {
        var tree = await yearTreeQueries.GetYearTreeAsync(request.YearId, cancellationToken);
        return tree is null ? Result.Fail<YearTreeResult>(CurriculumErrors.AcademicYearNotFound) : Result.Ok(tree);
    }
}

