using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.UnassignSubjectUsers;

public sealed record UnassignSubjectUsersCommand(int SubjectId, IReadOnlyList<int> UserIds) : ICommand<Result<NoValue>>;

public sealed class UnassignSubjectUsersCommandValidator : AbstractValidator<UnassignSubjectUsersCommand>
{
    public UnassignSubjectUsersCommandValidator()
    {
        RuleFor(x => x.SubjectId).GreaterThan(0);
        RuleForEach(x => x.UserIds).GreaterThan(0);
    }
}

public sealed class UnassignSubjectUsersCommandHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<UnassignSubjectUsersCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(UnassignSubjectUsersCommand request, CancellationToken cancellationToken)
    {
        if (await unitOfWork.Subjects.GetByIdAsync(request.SubjectId, cancellationToken) is null)
            return Result.Fail<NoValue>(CurriculumErrors.SubjectNotFound);
        await unitOfWork.SubjectUserAssignments.UnassignUsersAsync(request.SubjectId, request.UserIds, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok();
    }
}
