using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.UpdateSubject;

public sealed class UpdateSubjectCommandHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSubjectCommand, Result<SubjectDetailResult>>
{
    public async Task<Result<SubjectDetailResult>> Handle(UpdateSubjectCommand request, CancellationToken cancellationToken)
    {
        var subject = await unitOfWork.Subjects.GetByIdTrackedAsync(request.Id, cancellationToken);
        if (subject is null) return Result.Fail<SubjectDetailResult>(CurriculumErrors.SubjectNotFound);
        var updateResult = subject.Update(request.Name, request.Description);
        if (!updateResult.IsSuccess) return Result.Fail<SubjectDetailResult>(updateResult.Error);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(SubjectDetailResult.From(subject));
    }
}

