using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.SubjectGroups.UpdateSubjectGroup;

public sealed record UpdateSubjectGroupCommand(int Id, string Name) : ICommand<Result<SubjectGroupDetailResult>>;

public sealed class UpdateSubjectGroupCommandValidator : AbstractValidator<UpdateSubjectGroupCommand>
{
    public UpdateSubjectGroupCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public sealed class UpdateSubjectGroupCommandHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSubjectGroupCommand, Result<SubjectGroupDetailResult>>
{
    public async Task<Result<SubjectGroupDetailResult>> Handle(UpdateSubjectGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await unitOfWork.SubjectGroups.GetByIdTrackedAsync(request.Id, cancellationToken);
        if (group is null) return Result.Fail<SubjectGroupDetailResult>(CurriculumErrors.SubjectGroupNotFound);
        var updateResult = group.Update(request.Name);
        if (!updateResult.IsSuccess) return Result.Fail<SubjectGroupDetailResult>(updateResult.Error);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(SubjectGroupDetailResult.From(group));
    }
}
