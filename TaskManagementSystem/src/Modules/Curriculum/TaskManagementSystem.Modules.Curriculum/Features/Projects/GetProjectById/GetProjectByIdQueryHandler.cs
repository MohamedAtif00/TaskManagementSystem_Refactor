using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Projects.GetProjectById;

public sealed class GetProjectByIdQueryHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<GetProjectByIdQuery, Result<CurriculumProjectDetailResult>>
{
    public async Task<Result<CurriculumProjectDetailResult>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await unitOfWork.Projects.GetByIdAsync(request.Id, cancellationToken);
        return project is null ? Result.Fail<CurriculumProjectDetailResult>(CurriculumErrors.CurriculumProjectNotFound) : Result.Ok(CurriculumProjectDetailResult.From(project));
    }
}

