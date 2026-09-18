namespace TaskManagementSystem.BuildingBlocks.Domain;

public sealed record ResultError(
    string Code,
    string Message,
    IReadOnlyDictionary<string, string[]>? ValidationErrors = null);
