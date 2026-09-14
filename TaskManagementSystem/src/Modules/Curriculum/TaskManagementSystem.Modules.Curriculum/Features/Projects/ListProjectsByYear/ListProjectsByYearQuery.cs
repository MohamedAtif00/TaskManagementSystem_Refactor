using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Projects.ListProjectsByYear;

public sealed record ListProjectsByYearQuery(int YearId) : IQuery<Result<IReadOnlyList<CurriculumProjectListItemResult>>>;

public sealed class ListProjectsByYearQueryValidator : AbstractValidator<ListProjectsByYearQuery>
{
    public ListProjectsByYearQueryValidator() => RuleFor(x => x.YearId).GreaterThan(0);
}

public sealed class ListProjectsByYearQueryHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<ListProjectsByYearQuery, Result<IReadOnlyList<CurriculumProjectListItemResult>>>
{
    public async Task<Result<IReadOnlyList<CurriculumProjectListItemResult>>> Handle(ListProjectsByYearQuery request, CancellationToken cancellationToken)
    {
        if (!await unitOfWork.Projects.ActiveYearExistsAsync(request.YearId, cancellationToken))
            return Result.Fail<IReadOnlyList<CurriculumProjectListItemResult>>(CurriculumErrors.AcademicYearNotFound);
        var projects = await unitOfWork.Projects.ListActiveByParentIdAsync(request.YearId, cancellationToken);
        return Result.Ok<IReadOnlyList<CurriculumProjectListItemResult>>(projects.Select(CurriculumProjectListItemResult.From).ToList());
    }
}
