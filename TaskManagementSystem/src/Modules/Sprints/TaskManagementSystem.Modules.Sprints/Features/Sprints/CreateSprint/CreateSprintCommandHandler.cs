using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Sprints.Application;
using TaskManagementSystem.Modules.Sprints.Domain;

namespace TaskManagementSystem.Modules.Sprints.Features.Sprints.CreateSprint;

public sealed class CreateSprintCommandHandler(ISprintsUnitOfWork unitOfWork)
    : IRequestHandler<CreateSprintCommand, Result<SprintDetailResult>>
{
    public async Task<Result<SprintDetailResult>> Handle(CreateSprintCommand request, CancellationToken cancellationToken)
    {
        var createResult = Sprint.Create(request.Name, request.Description, request.StartDate, request.EndDate);
        if (!createResult.IsSuccess)
        {
            return Result.Fail<SprintDetailResult>(createResult.Error);
        }

        await unitOfWork.Sprints.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(SprintDetailResult.From(createResult.Value, []));
    }
}

