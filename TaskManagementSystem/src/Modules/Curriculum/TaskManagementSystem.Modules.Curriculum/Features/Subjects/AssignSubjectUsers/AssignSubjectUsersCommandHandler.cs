using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.AssignSubjectUsers;

public sealed class AssignSubjectUsersCommandHandler(ICurriculumUnitOfWork unitOfWork, IIdentityUserLookup identityUserLookup)
    : IRequestHandler<AssignSubjectUsersCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(AssignSubjectUsersCommand request, CancellationToken cancellationToken)
    {
        if (await unitOfWork.Subjects.GetByIdAsync(request.SubjectId, cancellationToken) is null)
            return Result.Fail<NoValue>(CurriculumErrors.SubjectNotFound);
        if (request.UserIds.Count > 0 && !await identityUserLookup.ActiveUsersExistAsync(request.UserIds, cancellationToken))
            return Result.Fail<NoValue>(CurriculumErrors.UserNotFound);
        await unitOfWork.SubjectUserAssignments.AssignUsersAsync(request.SubjectId, request.UserIds, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok();
    }
}

