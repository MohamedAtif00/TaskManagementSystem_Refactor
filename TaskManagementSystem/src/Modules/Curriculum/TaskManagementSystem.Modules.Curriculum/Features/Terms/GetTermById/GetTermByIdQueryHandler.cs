using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Terms.GetTermById;

public sealed class GetTermByIdQueryHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<GetTermByIdQuery, Result<CurriculumTermDetailResult>>
{
    public async Task<Result<CurriculumTermDetailResult>> Handle(GetTermByIdQuery request, CancellationToken cancellationToken)
    {
        var term = await unitOfWork.Terms.GetByIdAsync(request.Id, cancellationToken);
        return term is null ? Result.Fail<CurriculumTermDetailResult>(CurriculumErrors.CurriculumTermNotFound) : Result.Ok(CurriculumTermDetailResult.From(term));
    }
}

