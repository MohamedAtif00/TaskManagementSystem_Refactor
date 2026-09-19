using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Terms.UpdateTerm;

public sealed class UpdateTermCommandHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<UpdateTermCommand, Result<CurriculumTermDetailResult>>
{
    public async Task<Result<CurriculumTermDetailResult>> Handle(UpdateTermCommand request, CancellationToken cancellationToken)
    {
        var term = await unitOfWork.Terms.GetByIdTrackedAsync(request.Id, cancellationToken);
        if (term is null) return Result.Fail<CurriculumTermDetailResult>(CurriculumErrors.CurriculumTermNotFound);
        var updateResult = term.Update(request.Name, request.StartDate, request.EndDate);
        if (!updateResult.IsSuccess) return Result.Fail<CurriculumTermDetailResult>(updateResult.Error);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(CurriculumTermDetailResult.From(term));
    }
}

