using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;

namespace TaskManagementSystem.TestCommon.MediatR;

public sealed record ValidatedCommand(string Name) : ICommand<string>;

public sealed class ValidatedCommandHandler : IRequestHandler<ValidatedCommand, string>
{
    public Task<string> Handle(ValidatedCommand request, CancellationToken cancellationToken) =>
        Task.FromResult($"Hello, {request.Name}");
}

public sealed class ValidatedCommandValidator : AbstractValidator<ValidatedCommand>
{
    public ValidatedCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .WithMessage("Name is required.");
    }
}
