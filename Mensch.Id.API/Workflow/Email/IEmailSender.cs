using System.Threading.Tasks;

namespace Mensch.Id.API.Workflow.Email
{
    public interface IEmailSender
    {
        Task SendVerificationEmail(VerificationEmail email);
        Task SendPasswordResetEmail(PasswordResetEmail email);
    }
}