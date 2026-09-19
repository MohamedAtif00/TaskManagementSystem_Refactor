using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Terms.ArchiveTerm;

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

