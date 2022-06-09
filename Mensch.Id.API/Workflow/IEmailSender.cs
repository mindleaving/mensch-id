using System.Threading.Tasks;

namespace Mensch.Id.API.Workflow
{
    public interface IEmailSender
    {
        Task SendVerificationEmail(
            VerificationEmail email);

        Task SendPasswordResetEmail(
            PasswordResetEmail email);
    }
}