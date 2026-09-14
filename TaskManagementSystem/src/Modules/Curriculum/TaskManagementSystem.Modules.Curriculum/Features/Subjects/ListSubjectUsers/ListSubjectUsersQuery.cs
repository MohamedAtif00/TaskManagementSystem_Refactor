using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.ListSubjectUsers;

public sealed record ListSubjectUsersQuery(int SubjectId) : IQuery<Result<IReadOnlyList<SubjectUserResult>>>;

public sealed class ListSubjectUsersQueryValidator : AbstractValidator<ListSubjectUsersQuery>
{
    public ListSubjectUsersQueryValidator() => RuleFor(x => x.SubjectId).GreaterThan(0);
}

public sealed class ListSubjectUsersQueryHandler(ICurriculumUnitOfWork unitOfWork, IIdentityUserLookup identityUserLookup)
    : IRequestHandler<ListSubjectUsersQuery, Result<IReadOnlyList<SubjectUserResult>>>
{
    public async Task<Result<IReadOnlyList<SubjectUserResult>>> Handle(ListSubjectUsersQuery request, CancellationToken cancellationToken)
    {
        if (await unitOfWork.Subjects.GetByIdAsync(request.SubjectId, cancellationToken) is null)
            return Result.Fail<IReadOnlyList<SubjectUserResult>>(CurriculumErrors.SubjectNotFound);
        var userIds = await unitOfWork.SubjectUserAssignments.ListUserIdsBySubjectIdAsync(request.SubjectId, cancellationToken);
        var users = await identityUserLookup.GetActiveUsersByIdsAsync(userIds, cancellationToken);
        return Result.Ok<IReadOnlyList<SubjectUserResult>>(users.Select(user => new SubjectUserResult(user.Id, user.Name)).ToList());
    }
}
