using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Features.SubjectGroups.CreateSubjectGroup;

public sealed record CreateSubjectGroupCommand(int TermId, string Name) : ICommand<Result<SubjectGroupDetailResult>>;

public sealed class CreateSubjectGroupCommandValidator : AbstractValidator<CreateSubjectGroupCommand>
{
    public CreateSubjectGroupCommandValidator()
    {
        RuleFor(x => x.TermId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public sealed class CreateSubjectGroupCommandHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<CreateSubjectGroupCommand, Result<SubjectGroupDetailResult>>
{
    public async Task<Result<SubjectGroupDetailResult>> Handle(CreateSubjectGroupCommand request, CancellationToken cancellationToken)
    {
        if (await unitOfWork.SubjectGroups.ArchivedTermExistsAsync(request.TermId, cancellationToken))
            return Result.Fail<SubjectGroupDetailResult>(CurriculumErrors.ParentArchived);
        if (!await unitOfWork.SubjectGroups.ActiveTermExistsAsync(request.TermId, cancellationToken))
            return Result.Fail<SubjectGroupDetailResult>(CurriculumErrors.CurriculumTermNotFound);
        var createResult = SubjectGroup.Create(request.Name, request.TermId);
        if (!createResult.IsSuccess) return Result.Fail<SubjectGroupDetailResult>(createResult.Error);
        await unitOfWork.SubjectGroups.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(SubjectGroupDetailResult.From(createResult.Value));
    }
}
