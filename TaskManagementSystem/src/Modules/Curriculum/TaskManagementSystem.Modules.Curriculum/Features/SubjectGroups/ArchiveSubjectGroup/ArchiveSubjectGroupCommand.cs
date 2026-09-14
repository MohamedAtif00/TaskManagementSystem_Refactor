using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.SubjectGroups.ArchiveSubjectGroup;

public sealed record ArchiveSubjectGroupCommand(int Id) : ICommand<Result<NoValue>>;

public sealed class ArchiveSubjectGroupCommandValidator : AbstractValidator<ArchiveSubjectGroupCommand>
{
    public ArchiveSubjectGroupCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public sealed class ArchiveSubjectGroupCommandHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<ArchiveSubjectGroupCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(ArchiveSubjectGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await unitOfWork.SubjectGroups.GetByIdTrackedAsync(request.Id, cancellationToken);
        if (group is null) return Result.Fail<NoValue>(CurriculumErrors.SubjectGroupNotFound);
        var archiveResult = group.Archive();
        if (!archiveResult.IsSuccess) return archiveResult;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok();
    }
}
