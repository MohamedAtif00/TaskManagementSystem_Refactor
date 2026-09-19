using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Sprints.Application;

namespace TaskManagementSystem.Modules.Sprints.Features.Sprints.ArchiveSprint;

public sealed class ArchiveSprintCommandHandler(ISprintsUnitOfWork unitOfWork)
    : IRequestHandler<ArchiveSprintCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(ArchiveSprintCommand request, CancellationToken cancellationToken)
    {
        var sprint = await unitOfWork.Sprints.GetByIdTrackedAsync(request.Id, cancellationToken);
        if (sprint is null)
        {
            return Result.Fail<NoValue>(SprintsErrors.SprintNotFound);
        }

        var archiveResult = sprint.Archive();
        if (!archiveResult.IsSuccess)
        {
            return archiveResult;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok();
    }
}

