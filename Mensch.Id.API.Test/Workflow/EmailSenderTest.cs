using System;
using System.IO;
using System.Threading.Tasks;
using Mensch.Id.API.Models;
using Mensch.Id.API.Workflow.Email;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Mensch.Id.API.Test.Workflow
{
    public class EmailSenderTest
    {
        [Test]
        public async Task SendEmailViaExternalServer()
        {
            var settings = new EmailSettings
            {
                FromAddress = "noreply@mensch.id",
                SmtpServerName = "send.one.com",
                SmtpServerPort = 465,
                UseAuthentication = true,
                SmtpUsername = "noreply@mensch.id",
                SmtpPassword = await File.ReadAllTextAsync(@"C:\temp\smtp-password.txt"),
                WebsiteBaseUrl = "https://test.mensch.id"
            };
            var wrappedSettings = new OptionsWrapper<EmailSettings>(settings);
            var sut = new EmailSender(wrappedSettings, new EmailComposer(wrappedSettings));
            //var email = new OrderDigestEmail
            //{
            //    OpenOrdersCount = 2
            //};
            
            //await sut.SendOrderDigestEmail(email);

            var email = new VerificationEmail
            {
                AccountId = Guid.NewGuid().ToString(),
                PreferedLanguage = Language.en,
                RecipientAddress = "mindleaving@gmail.com",
                VerificationToken = Guid.NewGuid().ToString()
            };
            await sut.SendVerificationEmail(email);
        }
    }
}
