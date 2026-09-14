using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.SubjectGroups.ListSubjectGroupsByTerm;

public sealed record ListSubjectGroupsByTermQuery(int TermId) : IQuery<Result<IReadOnlyList<SubjectGroupListItemResult>>>;

public sealed class ListSubjectGroupsByTermQueryValidator : AbstractValidator<ListSubjectGroupsByTermQuery>
{
    public ListSubjectGroupsByTermQueryValidator() => RuleFor(x => x.TermId).GreaterThan(0);
}

public sealed class ListSubjectGroupsByTermQueryHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<ListSubjectGroupsByTermQuery, Result<IReadOnlyList<SubjectGroupListItemResult>>>
{
    public async Task<Result<IReadOnlyList<SubjectGroupListItemResult>>> Handle(ListSubjectGroupsByTermQuery request, CancellationToken cancellationToken)
    {
        if (!await unitOfWork.SubjectGroups.ActiveTermExistsAsync(request.TermId, cancellationToken))
            return Result.Fail<IReadOnlyList<SubjectGroupListItemResult>>(CurriculumErrors.CurriculumTermNotFound);
        var groups = await unitOfWork.SubjectGroups.ListActiveByParentIdAsync(request.TermId, cancellationToken);
        return Result.Ok<IReadOnlyList<SubjectGroupListItemResult>>(groups.Select(SubjectGroupListItemResult.From).ToList());
    }
}
