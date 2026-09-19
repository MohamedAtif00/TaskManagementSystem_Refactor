using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.CreateSubject;

public sealed class CreateSubjectCommandHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<CreateSubjectCommand, Result<SubjectDetailResult>>
{
    public async Task<Result<SubjectDetailResult>> Handle(CreateSubjectCommand request, CancellationToken cancellationToken)
    {
        if (await unitOfWork.Subjects.ArchivedSubjectGroupExistsAsync(request.SubjectGroupId, cancellationToken))
            return Result.Fail<SubjectDetailResult>(CurriculumErrors.ParentArchived);
        if (!await unitOfWork.Subjects.ActiveSubjectGroupExistsAsync(request.SubjectGroupId, cancellationToken))
            return Result.Fail<SubjectDetailResult>(CurriculumErrors.SubjectGroupNotFound);
        var createResult = Subject.Create(request.Name, request.Description, request.SubjectGroupId);
        if (!createResult.IsSuccess) return Result.Fail<SubjectDetailResult>(createResult.Error);
        await unitOfWork.Subjects.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(SubjectDetailResult.From(createResult.Value));
    }
}

