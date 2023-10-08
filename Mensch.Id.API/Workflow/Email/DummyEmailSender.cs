using System;
using System.Threading.Tasks;
using MimeKit;

namespace Mensch.Id.API.Workflow.Email
{
    public class DummyEmailSender : IEmailSender
    {
        private readonly EmailComposer emailComposer;

        public DummyEmailSender(
            EmailComposer emailComposer)
        {
            this.emailComposer = emailComposer;
        }

        public Task SendVerificationEmail(
            VerificationEmail email)
        {
            var message = emailComposer.Compose(email);
            WriteToConsole(message);
            return Task.CompletedTask;
        }

        public Task SendPasswordResetEmail(
            PasswordResetEmail email)
        {
            var message = emailComposer.Compose(email);
            WriteToConsole(message);
            return Task.CompletedTask;
        }

        public Task SendAssignerAccountApprovedEmail(
            AssignerAccountRequestApprovedEmail email)
        {
            var message = emailComposer.Compose(email);
            WriteToConsole(message);
            return Task.CompletedTask;
        }

        public Task SendOrderDigestEmail(
            OrderDigestEmail email)
        {
            var message = emailComposer.Compose(email);
            WriteToConsole(message);
            return Task.CompletedTask;
        }

        private void WriteToConsole(
            MimeMessage message)
        {
            Console.WriteLine($"From: {message.From}");
            Console.WriteLine($"To: {message.To}");
            Console.WriteLine($"Subject: {message.Subject}");
            Console.WriteLine($"Message: {message.TextBody}");
        }
    }
}
