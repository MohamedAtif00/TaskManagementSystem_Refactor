using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Terms.ArchiveTerm;

public sealed record ArchiveTermCommand(int Id) : ICommand<Result<NoValue>>;

public sealed class ArchiveTermCommandValidator : AbstractValidator<ArchiveTermCommand>
{
    public ArchiveTermCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public sealed class ArchiveTermCommandHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<ArchiveTermCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(ArchiveTermCommand request, CancellationToken cancellationToken)
    {
        var term = await unitOfWork.Terms.GetByIdTrackedAsync(request.Id, cancellationToken);
        if (term is null) return Result.Fail<NoValue>(CurriculumErrors.CurriculumTermNotFound);
        var archiveResult = term.Archive();
        if (!archiveResult.IsSuccess) return archiveResult;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok();
    }
}
