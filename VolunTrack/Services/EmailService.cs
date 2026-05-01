using Microsoft.Extensions.Options;
using VolunTrack.Models;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;

namespace VolunTrack.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendReminderAsync(string toEmail, string eventName, DateTime eventDate)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("VolunTrack", _settings.SenderEmail));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = $"Напоминание: событие '{eventName}' уже завтра!";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                    <h2>Доброе утро!</h2>
                    <p>Напоминаем, что завтра, {eventDate:dd.MM.yyyy HH:mm}, состоится событие <b>'{eventName}'</b>.</p>
                    <p>Ждём вас!</p>"
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                
                await client.ConnectAsync(_settings.SmtpServer, _settings.Port, SecureSocketOptions.StartTls);
                
                await client.AuthenticateAsync(_settings.SenderEmail, _settings.AppPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                _logger.LogInformation("Reminder sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send reminder email to {Email}", toEmail);
                throw;
            }
        }
    }
}
