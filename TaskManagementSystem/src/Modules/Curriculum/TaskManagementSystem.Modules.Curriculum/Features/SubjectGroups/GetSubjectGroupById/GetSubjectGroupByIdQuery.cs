using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.SubjectGroups.GetSubjectGroupById;

public sealed record GetSubjectGroupByIdQuery(int Id) : IQuery<Result<SubjectGroupDetailResult>>;

public sealed class GetSubjectGroupByIdQueryHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<GetSubjectGroupByIdQuery, Result<SubjectGroupDetailResult>>
{
    public async Task<Result<SubjectGroupDetailResult>> Handle(GetSubjectGroupByIdQuery request, CancellationToken cancellationToken)
    {
        var group = await unitOfWork.SubjectGroups.GetByIdAsync(request.Id, cancellationToken);
        return group is null ? Result.Fail<SubjectGroupDetailResult>(CurriculumErrors.SubjectGroupNotFound) : Result.Ok(SubjectGroupDetailResult.From(group));
    }
}
