using System.Collections.Concurrent;
using System.Threading.Tasks;
using Mensch.Id.API.Workflow.Email;

namespace Mensch.Id.API.Test.TestInfrastructure;

public class CapturingEmailSender : IEmailSender
{
    public ConcurrentBag<VerificationEmail> VerificationEmails { get; } = new();
    public ConcurrentBag<PasswordResetEmail> PasswordResetEmails { get; } = new();
    public ConcurrentBag<ExternalServiceVerificationEmail> ExternalServiceVerificationEmails { get; } = new();

    public Task SendExternalServiceVerificationEmail(ExternalServiceVerificationEmail email)
    {
        ExternalServiceVerificationEmails.Add(email);
        return Task.CompletedTask;
    }

    public Task SendVerificationEmail(VerificationEmail email)
    {
        VerificationEmails.Add(email);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetEmail(PasswordResetEmail email)
    {
        PasswordResetEmails.Add(email);
        return Task.CompletedTask;
    }

    public Task SendAssignerAccountApprovedEmail(AssignerAccountRequestApprovedEmail email)
    {
        return Task.CompletedTask;
    }

    public Task SendOrderDigestEmail(OrderDigestEmail email)
    {
        return Task.CompletedTask;
    }
}
