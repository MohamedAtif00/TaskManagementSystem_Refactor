using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Features.Terms.CreateTerm;

public sealed record CreateTermCommand(int ProjectId, string Name, DateTime? StartDate, DateTime? EndDate) : ICommand<Result<CurriculumTermDetailResult>>;

public sealed class CreateTermCommandValidator : AbstractValidator<CreateTermCommand>
{
    public CreateTermCommandValidator()
    {
        RuleFor(x => x.ProjectId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public sealed class CreateTermCommandHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<CreateTermCommand, Result<CurriculumTermDetailResult>>
{
    public async Task<Result<CurriculumTermDetailResult>> Handle(CreateTermCommand request, CancellationToken cancellationToken)
    {
        if (await unitOfWork.Terms.ArchivedProjectExistsAsync(request.ProjectId, cancellationToken))
            return Result.Fail<CurriculumTermDetailResult>(CurriculumErrors.ParentArchived);
        if (!await unitOfWork.Terms.ActiveProjectExistsAsync(request.ProjectId, cancellationToken))
            return Result.Fail<CurriculumTermDetailResult>(CurriculumErrors.CurriculumProjectNotFound);
        var createResult = CurriculumTerm.Create(request.Name, request.StartDate, request.EndDate, request.ProjectId);
        if (!createResult.IsSuccess) return Result.Fail<CurriculumTermDetailResult>(createResult.Error);
        await unitOfWork.Terms.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(CurriculumTermDetailResult.From(createResult.Value));
    }
}
