using System.Collections.Concurrent;
using System.Reflection;
using FluentValidation.Results;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.BuildingBlocks.Application.Behaviors;

internal static class ValidationResultFactory
{
    private static readonly ConcurrentDictionary<Type, Func<ResultError, object>> Factories = new();

    public static object CreateFailure(Type responseType, IReadOnlyList<ValidationFailure> failures)
    {
        var factory = Factories.GetOrAdd(responseType, CreateFactory);
        var groupedErrors = ValidationErrorGrouping.GroupByProperty(failures);
        var message = failures[0].ErrorMessage;
        return factory(new ResultError("validation_failed", message, groupedErrors));
    }

    private static Func<ResultError, object> CreateFactory(Type responseType)
    {
        if (!responseType.IsGenericType || responseType.GetGenericTypeDefinition() != typeof(Result<>))
        {
            throw new InvalidOperationException($"Cannot create validation failure for type '{responseType.Name}'.");
        }

        var valueType = responseType.GetGenericArguments()[0];
        var failMethod = typeof(Result)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(method =>
                method.Name == nameof(Result.Fail)
                && method.IsGenericMethodDefinition
                && method.GetParameters().Length == 1
                && method.GetParameters()[0].ParameterType == typeof(ResultError));

        var genericFail = failMethod.MakeGenericMethod(valueType);

        return error => genericFail.Invoke(null, [error])!;
    }
}
