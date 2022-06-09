using System.Threading.Tasks;

namespace Mensch.Id.API.Workflow
{
    public class DummyEmailSender : IEmailSender
    {
        public Task SendVerificationEmail(
            VerificationEmail email)
        {
            // Do nothing
            return Task.CompletedTask;
        }

        public Task SendPasswordResetEmail(
            PasswordResetEmail email)
        {
            // Do nothing
            return Task.CompletedTask;
        }
    }
}
