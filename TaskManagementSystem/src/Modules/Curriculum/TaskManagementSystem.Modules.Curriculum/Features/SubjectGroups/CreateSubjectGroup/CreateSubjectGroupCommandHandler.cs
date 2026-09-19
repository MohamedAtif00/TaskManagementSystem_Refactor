using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Features.SubjectGroups.CreateSubjectGroup;

public sealed class CreateSubjectGroupCommandHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<CreateSubjectGroupCommand, Result<SubjectGroupDetailResult>>
{
    public async Task<Result<SubjectGroupDetailResult>> Handle(CreateSubjectGroupCommand request, CancellationToken cancellationToken)
    {
        if (await unitOfWork.SubjectGroups.ArchivedTermExistsAsync(request.TermId, cancellationToken))
            return Result.Fail<SubjectGroupDetailResult>(CurriculumErrors.ParentArchived);
        if (!await unitOfWork.SubjectGroups.ActiveTermExistsAsync(request.TermId, cancellationToken))
            return Result.Fail<SubjectGroupDetailResult>(CurriculumErrors.CurriculumTermNotFound);
        var createResult = SubjectGroup.Create(request.Name, request.TermId);
        if (!createResult.IsSuccess) return Result.Fail<SubjectGroupDetailResult>(createResult.Error);
        await unitOfWork.SubjectGroups.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(SubjectGroupDetailResult.From(createResult.Value));
    }
}

