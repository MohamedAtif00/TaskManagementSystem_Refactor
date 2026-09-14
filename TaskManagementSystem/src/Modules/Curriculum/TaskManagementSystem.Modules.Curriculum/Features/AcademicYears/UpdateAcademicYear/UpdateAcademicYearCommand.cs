using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.AcademicYears.UpdateAcademicYear;

public sealed record UpdateAcademicYearCommand(int Id, string Name, string? Description) : ICommand<Result<AcademicYearDetailResult>>;

public sealed class UpdateAcademicYearCommandValidator : AbstractValidator<UpdateAcademicYearCommand>
{
    public UpdateAcademicYearCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public sealed class UpdateAcademicYearCommandHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<UpdateAcademicYearCommand, Result<AcademicYearDetailResult>>
{
    public async Task<Result<AcademicYearDetailResult>> Handle(UpdateAcademicYearCommand request, CancellationToken cancellationToken)
    {
        var year = await unitOfWork.AcademicYears.GetByIdTrackedAsync(request.Id, cancellationToken);
        if (year is null) return Result.Fail<AcademicYearDetailResult>(CurriculumErrors.AcademicYearNotFound);
        var updateResult = year.Update(request.Name, request.Description);
        if (!updateResult.IsSuccess) return Result.Fail<AcademicYearDetailResult>(updateResult.Error);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(AcademicYearDetailResult.From(year));
    }
}
