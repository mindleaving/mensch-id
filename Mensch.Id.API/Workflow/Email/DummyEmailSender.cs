﻿using System.Threading.Tasks;

namespace Mensch.Id.API.Workflow.Email
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

        public Task SendOrderDigestEmail(
            OrderDigestEmail email)
        {
            // Do nothing
            return Task.CompletedTask;
        }
    }
}
