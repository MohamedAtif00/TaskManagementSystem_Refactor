using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.UpdateSubjectStatus;

public sealed class UpdateSubjectStatusCommandHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSubjectStatusCommand, Result<SubjectDetailResult>>
{
    public async Task<Result<SubjectDetailResult>> Handle(UpdateSubjectStatusCommand request, CancellationToken cancellationToken)
    {
        var subject = await unitOfWork.Subjects.GetByIdTrackedAsync(request.SubjectId, cancellationToken);
        if (subject is null) return Result.Fail<SubjectDetailResult>(CurriculumErrors.SubjectNotFound);
        var updateResult = subject.UpdateStatus(request.Status);
        if (!updateResult.IsSuccess) return Result.Fail<SubjectDetailResult>(updateResult.Error);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(SubjectDetailResult.From(subject));
    }
}

