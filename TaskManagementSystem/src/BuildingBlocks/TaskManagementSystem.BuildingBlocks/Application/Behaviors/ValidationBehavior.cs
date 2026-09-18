using System.Text;
using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.BuildingBlocks.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);
        var failures = validators
            .Select(validator => validator.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(error => error is not null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next(cancellationToken);
        }

        if (typeof(TResponse).IsGenericType
            && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            return (TResponse)ValidationResultFactory.CreateFailure(typeof(TResponse), failures);
        }

        var message = new StringBuilder("Validation failed:");
        foreach (var failure in failures)
        {
            message.AppendLine();
            message.Append(failure.ErrorMessage);
        }

        throw new ValidationException(message.ToString(), failures);
    }
}
