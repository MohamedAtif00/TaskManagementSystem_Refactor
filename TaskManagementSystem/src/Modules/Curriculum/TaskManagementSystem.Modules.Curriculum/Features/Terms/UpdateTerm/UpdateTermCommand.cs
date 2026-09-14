using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Terms.UpdateTerm;

public sealed record UpdateTermCommand(int Id, string Name, DateTime? StartDate, DateTime? EndDate) : ICommand<Result<CurriculumTermDetailResult>>;

public sealed class UpdateTermCommandValidator : AbstractValidator<UpdateTermCommand>
{
    public UpdateTermCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

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
