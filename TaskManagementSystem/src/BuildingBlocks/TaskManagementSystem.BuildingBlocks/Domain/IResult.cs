namespace TaskManagementSystem.BuildingBlocks.Domain;

public interface IResult
{
    bool IsSuccess { get; }

    bool IsFailure { get; }
}
