using FluentValidation;

namespace TaskManagementSystem.Modules.Ticket.Features.Comments.AddComment;

public sealed class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentCommandValidator()
    {
        RuleFor(x => x.TicketId).GreaterThan(0);
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.Content).NotEmpty();
    }
}

