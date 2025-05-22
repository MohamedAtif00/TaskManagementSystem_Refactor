using AutomatedTaskSystem.Dtos;

namespace AutomatedTaskSystem.Services.Email
{
    public interface IEmailService
    {
        Task<EmailResult> SendEmailAsync(EmailMessage message);
    }
}
