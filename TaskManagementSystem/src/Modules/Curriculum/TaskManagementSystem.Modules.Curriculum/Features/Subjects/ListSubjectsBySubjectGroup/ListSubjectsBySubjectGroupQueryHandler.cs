using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.ListSubjectsBySubjectGroup;

public sealed class ListSubjectsBySubjectGroupQueryHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<ListSubjectsBySubjectGroupQuery, Result<IReadOnlyList<SubjectListItemResult>>>
{
    public async Task<Result<IReadOnlyList<SubjectListItemResult>>> Handle(ListSubjectsBySubjectGroupQuery request, CancellationToken cancellationToken)
    {
        if (!await unitOfWork.Subjects.ActiveSubjectGroupExistsAsync(request.SubjectGroupId, cancellationToken))
            return Result.Fail<IReadOnlyList<SubjectListItemResult>>(CurriculumErrors.SubjectGroupNotFound);
        var subjects = await unitOfWork.Subjects.ListActiveByParentIdAsync(request.SubjectGroupId, cancellationToken);
        return Result.Ok<IReadOnlyList<SubjectListItemResult>>(subjects.Select(SubjectListItemResult.From).ToList());
    }
}

