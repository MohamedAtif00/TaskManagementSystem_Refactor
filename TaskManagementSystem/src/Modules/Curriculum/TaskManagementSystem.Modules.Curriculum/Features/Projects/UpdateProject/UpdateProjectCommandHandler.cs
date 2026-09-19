using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Projects.UpdateProject;

public sealed class UpdateProjectCommandHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<UpdateProjectCommand, Result<CurriculumProjectDetailResult>>
{
    public async Task<Result<CurriculumProjectDetailResult>> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await unitOfWork.Projects.GetByIdTrackedAsync(request.Id, cancellationToken);
        if (project is null) return Result.Fail<CurriculumProjectDetailResult>(CurriculumErrors.CurriculumProjectNotFound);
        var updateResult = project.Update(request.Name, request.Description);
        if (!updateResult.IsSuccess) return Result.Fail<CurriculumProjectDetailResult>(updateResult.Error);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(CurriculumProjectDetailResult.From(project));
    }
}

