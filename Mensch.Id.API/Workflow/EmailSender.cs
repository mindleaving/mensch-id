using System;
using System.Threading.Tasks;

namespace Mensch.Id.API.Workflow
{
    public class EmailSender : IEmailSender
    {
        public async Task SendVerificationEmail(
            VerificationEmail email)
        {
            throw new NotImplementedException();
        }

        public async Task SendPasswordResetEmail(
            PasswordResetEmail email)
        {
            throw new NotImplementedException();
        }
    }

    public class PasswordResetEmail
    {
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string AccountId { get; set; }
        public string ResetToken { get; set; }
    }

    public class VerificationEmail
    {
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string AccountId { get; set; }
        public string VerificationToken { get; set; }
    }
}
