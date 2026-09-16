using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace Persistence.Email
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendContactConfirmationAsync(string toEmail, string name, string subject, string message)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = $"We received your message: {subject}";

            email.Body = new TextPart("plain")
            {
                Text =
                    $"Hi {name},\n\n" +
                    $"Thanks for reaching out. Here's a copy of what you sent us:\n\n" +
                    $"Subject: {subject}\n" +
                    $"Message: {message}\n\n" +
                    $"We'll get back to you soon.\n"
            };

            using var client = new SmtpClient();

            // Gmail SMTP: smtp.gmail.com, port 587, StartTls
            await client.ConnectAsync(
                _settings.SmtpHost,
                _settings.SmtpPort,
                _settings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

           
            await client.AuthenticateAsync(_settings.SenderEmail, _settings.Password);

            await client.SendAsync(email);
            await client.DisconnectAsync(true);
        }

        public async Task SendEmailAsync(string To, string Subject, string Body)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            emailMessage.To.Add(MailboxAddress.Parse(To));
            emailMessage.Subject = Subject;

            emailMessage.Body = new TextPart(TextFormat.Html)
            {
                Text = Body
            };
            try
            {
                using var smtp = new SmtpClient();
                //connect
                await smtp.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls);
                //authenticate
                await smtp.AuthenticateAsync(_settings.SenderEmail, _settings.Password);

                //send email
                await smtp.SendAsync(emailMessage);
                //Disconnect
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Email sending failed. Please try again later.",
                    ex
                );
            }

        }
    }
}
