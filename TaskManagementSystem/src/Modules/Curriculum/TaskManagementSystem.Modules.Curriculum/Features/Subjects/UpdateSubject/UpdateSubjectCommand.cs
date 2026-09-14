using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.UpdateSubject;

public sealed record UpdateSubjectCommand(int Id, string Name, string Description) : ICommand<Result<SubjectDetailResult>>;

public sealed class UpdateSubjectCommandValidator : AbstractValidator<UpdateSubjectCommand>
{
    public UpdateSubjectCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public sealed class UpdateSubjectCommandHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSubjectCommand, Result<SubjectDetailResult>>
{
    public async Task<Result<SubjectDetailResult>> Handle(UpdateSubjectCommand request, CancellationToken cancellationToken)
    {
        var subject = await unitOfWork.Subjects.GetByIdTrackedAsync(request.Id, cancellationToken);
        if (subject is null) return Result.Fail<SubjectDetailResult>(CurriculumErrors.SubjectNotFound);
        var updateResult = subject.Update(request.Name, request.Description);
        if (!updateResult.IsSuccess) return Result.Fail<SubjectDetailResult>(updateResult.Error);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(SubjectDetailResult.From(subject));
    }
}
