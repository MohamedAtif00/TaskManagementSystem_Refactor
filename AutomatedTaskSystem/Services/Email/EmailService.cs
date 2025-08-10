using AutomatedTaskSystem.Dtos;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging; // Ensure this is included
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

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
            IConfiguration configuration, // IConfiguration is needed to get the username
            ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
            // It's generally better to get both username and password from configuration
            // to avoid hardcoding sensitive information.
            _smtpUsername = configuration["EmailCredentials:Username"];
            _smtpPassword = configuration["EmailCredentials:Password"] ?? "drlc7n_Z-dud72!O*lko69ke+"; // Fallback for demonstration
        }

        /// <summary>
        /// Sends a single email asynchronously.
        /// </summary>
        /// <param name="message">The email message to send.</param>
        /// <returns>An EmailResult indicating success or failure.</returns>
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
                foreach (var cc in message.CcEmails)
                {
                    if (!string.IsNullOrWhiteSpace(cc)) // Ensure CC email is not null or empty
                    {
                        mailMessage.CC.Add(cc);
                    }
                }

                // Add attachments
                foreach (var attachment in message.Attachments)
                {
                    mailMessage.Attachments.Add(attachment);
                }

                using var smtpClient = new SmtpClient("smtp-mail.outlook.com")
                {
                    Port = _emailSettings.SmtpPort,
                    Credentials = new NetworkCredential("digital.leave@selaheltelmeez.com", "drlc7n_Z-dud72!O*lko69ke+"),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 30000 // 30 seconds
                };

                await smtpClient.SendMailAsync(mailMessage);

                result.Success = true;
                result.Message = "Email sent successfully";

                _logger.LogInformation("Email sent to {To} with {CcCount} CC recipients. Subject: {Subject}",
                    _emailSettings.StaticReceiver.Email,
                    mailMessage.CC.Count,
                    message.Subject);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Failed to send email. Subject: {message.Subject}. Error: {ex.Message}";
                result.Exception = ex;

                _logger.LogError(ex, "Email sending failed for subject: {Subject}", message.Subject);
            }

            return result;
        }

        /// <summary>
        /// Sends multiple emails asynchronously.
        /// </summary>
        /// <param name="messages">A collection of email messages to send.</param>
        /// <returns>A BulkEmailResult summarizing the outcome of the bulk operation.</returns>
        public async Task<BulkEmailResult> SendBulkEmailAsync(IEnumerable<EmailMessage> messages)
        {
            // Corrected: Initialize bulkResult as BulkEmailResult, not EmailResult
            var bulkResult = new BulkEmailResult
            {
                TotalEmailsAttempted = messages.Count()
            };

            _logger.LogInformation("Attempting to send {Count} bulk emails.", bulkResult.TotalEmailsAttempted);

            foreach (var message in messages)
            {
                var individualResult = await SendEmailAsync(message);
                bulkResult.IndividualResults.Add(individualResult);

                if (individualResult.Success)
                {
                    bulkResult.SuccessfulEmails++;
                }
                else
                {
                    bulkResult.FailedEmails++;
                }
            }

            bulkResult.OverallSuccess = bulkResult.FailedEmails == 0;
            bulkResult.Message = bulkResult.OverallSuccess
                ? $"Successfully sent {bulkResult.SuccessfulEmails} out of {bulkResult.TotalEmailsAttempted} emails."
                : $"Sent {bulkResult.SuccessfulEmails} out of {bulkResult.TotalEmailsAttempted} emails. {bulkResult.FailedEmails} failed.";

            _logger.LogInformation("Bulk email operation completed. {Message}", bulkResult.Message);

            return bulkResult;
        }

        /// <summary>
        /// Validates the properties of an EmailMessage.
        /// </summary>
        /// <param name="message">The email message to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if the message is null.</exception>
        /// <exception cref="ArgumentException">Thrown if subject, body, or CC emails are invalid.</exception>
        private void ValidateMessage(EmailMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message), "Email message cannot be null.");

            if (string.IsNullOrWhiteSpace(message.Subject))
                throw new ArgumentException("Subject cannot be empty.", nameof(message.Subject));

            if (string.IsNullOrWhiteSpace(message.Body))
                throw new ArgumentException("Body cannot be empty.", nameof(message.Body));

            // Validate CC emails
            var emailAttr = new EmailAddressAttribute();
            foreach (var cc in message.CcEmails)
            {
                if (!string.IsNullOrWhiteSpace(cc) && !emailAttr.IsValid(cc))
                    throw new ArgumentException($"Invalid CC email address: {cc}", nameof(message.CcEmails));
            }
        }
    }
}
