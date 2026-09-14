using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.ArchiveSubject;

public sealed record ArchiveSubjectCommand(int Id) : ICommand<Result<NoValue>>;

public sealed class ArchiveSubjectCommandValidator : AbstractValidator<ArchiveSubjectCommand>
{
    public ArchiveSubjectCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public sealed class ArchiveSubjectCommandHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<ArchiveSubjectCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(ArchiveSubjectCommand request, CancellationToken cancellationToken)
    {
        var subject = await unitOfWork.Subjects.GetByIdTrackedAsync(request.Id, cancellationToken);
        if (subject is null) return Result.Fail<NoValue>(CurriculumErrors.SubjectNotFound);
        var archiveResult = subject.Archive();
        if (!archiveResult.IsSuccess) return archiveResult;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok();
    }
}
