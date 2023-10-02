using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Mensch.Id.API.Workflow.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings settings;
        private readonly EmailComposer emailComposer;

        public EmailSender(
            IOptions<EmailSettings> settings,
            EmailComposer emailComposer)
        {
            this.emailComposer = emailComposer;
            this.settings = settings.Value;
        }

        public async Task SendVerificationEmail(
            VerificationEmail email)
        {
            var message = emailComposer.Compose(email);
            await SendMessage(message);
        }

        public async Task SendPasswordResetEmail(
            PasswordResetEmail email)
        {
            var message = emailComposer.Compose(email);
            await SendMessage(message);
        }

        public async Task SendAssignerAccountApprovedEmail(
            AssignerAccountRequestApprovedEmail email)
        {
            var message = emailComposer.Compose(email);
            await SendMessage(message);
        }

        public async Task SendOrderDigestEmail(
            OrderDigestEmail email)
        {
            var message = emailComposer.Compose(email);
            await SendMessage(message);
        }

        private async Task SendMessage(
            MimeMessage message)
        {
            try
            {
                Console.WriteLine("Sending email...");
                using var mailClient = new SmtpClient();
                await mailClient.ConnectAsync(settings.SmtpServerName, settings.SmtpServerPort, SecureSocketOptions.None);
                await mailClient.SendAsync(message);
                await mailClient.DisconnectAsync(true);
                Console.WriteLine("Email sent!");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not send email: {e.Message}");
                throw;
            }
        }
    }
}
