using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Projects.ArchiveProject;

public sealed class ArchiveProjectCommandHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<ArchiveProjectCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(ArchiveProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await unitOfWork.Projects.GetByIdTrackedAsync(request.Id, cancellationToken);
        if (project is null) return Result.Fail<NoValue>(CurriculumErrors.CurriculumProjectNotFound);
        var archiveResult = project.Archive();
        if (!archiveResult.IsSuccess) return archiveResult;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok();
    }
}

