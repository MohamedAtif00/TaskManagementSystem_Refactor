using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Features.Projects.CreateProject;

public sealed class CreateProjectCommandHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<CreateProjectCommand, Result<CurriculumProjectDetailResult>>
{
    public async Task<Result<CurriculumProjectDetailResult>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        if (await unitOfWork.Projects.ArchivedYearExistsAsync(request.YearId, cancellationToken))
            return Result.Fail<CurriculumProjectDetailResult>(CurriculumErrors.ParentArchived);
        if (!await unitOfWork.Projects.ActiveYearExistsAsync(request.YearId, cancellationToken))
            return Result.Fail<CurriculumProjectDetailResult>(CurriculumErrors.AcademicYearNotFound);
        var createResult = CurriculumProject.Create(request.Name, request.Description, request.YearId);
        if (!createResult.IsSuccess) return Result.Fail<CurriculumProjectDetailResult>(createResult.Error);
        await unitOfWork.Projects.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(CurriculumProjectDetailResult.From(createResult.Value));
    }
}

