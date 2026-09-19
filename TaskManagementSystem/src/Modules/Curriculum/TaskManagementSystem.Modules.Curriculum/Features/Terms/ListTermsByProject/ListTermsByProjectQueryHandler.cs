using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Terms.ListTermsByProject;

public sealed class ListTermsByProjectQueryHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<ListTermsByProjectQuery, Result<IReadOnlyList<CurriculumTermListItemResult>>>
{
    public async Task<Result<IReadOnlyList<CurriculumTermListItemResult>>> Handle(ListTermsByProjectQuery request, CancellationToken cancellationToken)
    {
        if (!await unitOfWork.Terms.ActiveProjectExistsAsync(request.ProjectId, cancellationToken))
            return Result.Fail<IReadOnlyList<CurriculumTermListItemResult>>(CurriculumErrors.CurriculumProjectNotFound);
        var terms = await unitOfWork.Terms.ListActiveByParentIdAsync(request.ProjectId, cancellationToken);
        return Result.Ok<IReadOnlyList<CurriculumTermListItemResult>>(terms.Select(CurriculumTermListItemResult.From).ToList());
    }
}

