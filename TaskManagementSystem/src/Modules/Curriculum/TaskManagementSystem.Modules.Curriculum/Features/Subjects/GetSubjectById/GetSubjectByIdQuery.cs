using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.GetSubjectById;

public sealed record GetSubjectByIdQuery(int Id) : IQuery<Result<SubjectDetailResult>>;

public sealed class GetSubjectByIdQueryHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<GetSubjectByIdQuery, Result<SubjectDetailResult>>
{
    public async Task<Result<SubjectDetailResult>> Handle(GetSubjectByIdQuery request, CancellationToken cancellationToken)
    {
        var subject = await unitOfWork.Subjects.GetByIdAsync(request.Id, cancellationToken);
        return subject is null ? Result.Fail<SubjectDetailResult>(CurriculumErrors.SubjectNotFound) : Result.Ok(SubjectDetailResult.From(subject));
    }
}
