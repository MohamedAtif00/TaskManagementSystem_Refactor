using AutomatedTaskSystem.Dtos;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using System.ComponentModel.DataAnnotations;

namespace AutomatedTaskSystem.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            IConfiguration configuration,
            ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
            _smtpUsername = configuration["EmailCredentials:Username"];
            _smtpPassword = "drlc7n_Z-dud72!O*lko69ke+";
            //_smtpPassword = "STP678345@#88$_";
        }

        public async Task<EmailResult> SendEmailAsync(EmailMessage message)
        {
            var result = new EmailResult();

            try
            {
                ValidateMessage(message);

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(
                        _emailSettings.StaticSender.Email,
                        _emailSettings.StaticSender.Name),
                    Subject = message.Subject,
                    Body = message.Body,
                    IsBodyHtml = message.IsHtml,
                    Priority = MailPriority.Normal
                };

                // Add static receiver
                mailMessage.To.Add(new MailAddress(
                    _emailSettings.StaticReceiver.Email,
                    _emailSettings.StaticReceiver.Name));

                // Add static CCs
                //foreach (var cc in message.CcEmails)
                //{
                //    mailMessage.CC.Add(cc);
                //}

                //// Add dynamic CCs
                //foreach (var cc in message.CcEmails)
                //{
                //    if (!mailMessage.CC.Any(c => c.Address.Equals(cc, StringComparison.OrdinalIgnoreCase)))
                //    {
                //        mailMessage.CC.Add(cc);
                //    }
                //}

                // Add attachments
                foreach (var attachment in message.Attachments)
                {
                    mailMessage.Attachments.Add(attachment);
                }

                using var smtpClient = new SmtpClient(_emailSettings.SmtpServer)
                {
                    Port = _emailSettings.SmtpPort,
                    Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                    EnableSsl = _emailSettings.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 30000 // 30 seconds
                };

                await smtpClient.SendMailAsync(mailMessage);

                result.Success = true;
                result.Message = "Email sent successfully";

                _logger.LogInformation("Email sent to {To} with {CcCount} CC recipients",
                    _emailSettings.StaticReceiver.Email,
                    mailMessage.CC.Count);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Failed to send email";
                result.Exception = ex;

                _logger.LogError(ex, "Email sending failed");
            }

            return result;
        }

        private void ValidateMessage(EmailMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (string.IsNullOrWhiteSpace(message.Subject))
                throw new ArgumentException("Subject cannot be empty");

            if (string.IsNullOrWhiteSpace(message.Body))
                throw new ArgumentException("Body cannot be empty");

            // Validate CC emails
            var emailAttr = new EmailAddressAttribute();
            foreach (var cc in message.CcEmails)
            {
                if (!emailAttr.IsValid(cc))
                    throw new ArgumentException($"Invalid CC email address: {cc}");
            }
        }
    }
}
