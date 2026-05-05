using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ims_inv.Services
{
    public class EmailSettings
    {
        public string Host { get; set; } = "";
        public int Port { get; set; }
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string From { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public bool EnableSsl { get; set; } = false;
    }

    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true, List<EmailAttachment>? attachments = null);
        Task SendTemplateEmailAsync<TModel>(string to, string subject, string viewName, TModel model, List<EmailAttachment>? attachments = null);
    }

    public class EmailAttachment
    {
        public string FileName { get; set; } = "";
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = "application/octet-stream";
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly IRazorViewRenderer _razorViewRenderer;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<EmailSettings> settings,
            IRazorViewRenderer razorViewRenderer,
            ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _razorViewRenderer = razorViewRenderer;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true, List<EmailAttachment>? attachments = null)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.DisplayName, _settings.From));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = isHtml ? body : null,
                TextBody = isHtml ? null : body
            };

            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    builder.Attachments.Add(attachment.FileName, attachment.Content, ContentType.Parse(attachment.ContentType));
                }
            }

            message.Body = builder.ToMessageBody();

            try
            {
                using var client = new SmtpClient();
                // Use SecureSocketOptions based on EnableSsl
                var socketOptions = _settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
                
                await client.ConnectAsync(_settings.Host, _settings.Port, socketOptions);
                
                if (!string.IsNullOrEmpty(_settings.Username))
                {
                    await client.AuthenticateAsync(_settings.Username, _settings.Password);
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {To}", to);
                throw;
            }
        }

        public async Task SendTemplateEmailAsync<TModel>(string to, string subject, string viewName, TModel model, List<EmailAttachment>? attachments = null)
        {
            var body = await _razorViewRenderer.RenderViewToStringAsync(viewName, model);
            await SendEmailAsync(to, subject, body, true, attachments);
        }
    }
}
