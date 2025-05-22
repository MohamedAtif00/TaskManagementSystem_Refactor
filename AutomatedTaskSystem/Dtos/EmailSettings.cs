    using System.ComponentModel.DataAnnotations;
    using System.Net.Mail;

    namespace AutomatedTaskSystem.Dtos
    {
        public class EmailSettings
        {
            public string SmtpServer { get; set; }
            public int SmtpPort { get; set; }
            public bool EnableSsl { get; set; }
            public EmailAddress StaticSender { get; set; }
            public EmailAddress StaticReceiver { get; set; }
        }

        public class EmailAddress
        {
            public string Email { get; set; }
            public string Name { get; set; }
        }

        public class EmailMessage
        {
            public string Subject { get; set; }
            public string Body { get; set; }
            public bool IsHtml { get; set; }
            public List<string> CcEmails { get; set; } = new List<string>();
            public List<Attachment> Attachments { get; set; } = new List<Attachment>();
        }

        public class EmailResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public Exception Exception { get; set; } // Optional, for logging/debugging purposes
        }


        public class SendEmailRequest
        {
            [Required(ErrorMessage = "Subject is required")]
            [StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters")]
            public string Subject { get; set; }

            [Required(ErrorMessage = "Body is required")]
            public string Body { get; set; }

            public bool IsHtml { get; set; } = false;

            [EmailListValidation(ErrorMessage = "One or more CC emails are invalid")]
            public List<string> CcEmails { get; set; } = new List<string>();
        }

        // Custom validation attribute for email lists
        public class EmailListValidationAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if (value is List<string> emails)
                {
                    var emailAttr = new EmailAddressAttribute();
                    foreach (var email in emails)
                    {
                        if (!emailAttr.IsValid(email))
                        {
                            return new ValidationResult($"Invalid email address: {email}");
                        }
                    }
                }
                return ValidationResult.Success;
            }
        }
    }
