using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.AssignSubjectUsers;

public sealed record AssignSubjectUsersCommand(int SubjectId, IReadOnlyList<int> UserIds) : ICommand<Result<NoValue>>;

public sealed class AssignSubjectUsersCommandValidator : AbstractValidator<AssignSubjectUsersCommand>
{
    public AssignSubjectUsersCommandValidator()
    {
        RuleFor(x => x.SubjectId).GreaterThan(0);
        RuleForEach(x => x.UserIds).GreaterThan(0);
    }
}

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
