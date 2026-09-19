using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.SubjectGroups.UpdateSubjectGroup;

public sealed class UpdateSubjectGroupCommandHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSubjectGroupCommand, Result<SubjectGroupDetailResult>>
{
    public async Task<Result<SubjectGroupDetailResult>> Handle(UpdateSubjectGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await unitOfWork.SubjectGroups.GetByIdTrackedAsync(request.Id, cancellationToken);
        if (group is null) return Result.Fail<SubjectGroupDetailResult>(CurriculumErrors.SubjectGroupNotFound);
        var updateResult = group.Update(request.Name);
        if (!updateResult.IsSuccess) return Result.Fail<SubjectGroupDetailResult>(updateResult.Error);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(SubjectGroupDetailResult.From(group));
    }
}

