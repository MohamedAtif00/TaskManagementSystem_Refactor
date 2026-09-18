using FluentValidation.Results;

namespace TaskManagementSystem.BuildingBlocks.Application.Behaviors;

public static class ValidationErrorGrouping
{
    public static Dictionary<string, string[]> GroupByProperty(IEnumerable<ValidationFailure> failures) =>
        failures
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());
}
