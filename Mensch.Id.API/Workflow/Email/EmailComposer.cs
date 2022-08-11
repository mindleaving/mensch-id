using System.Collections.Generic;
using System.Web;
using Mensch.Id.API.Models;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace Mensch.Id.API.Workflow.Email
{
    public class EmailComposer
    {
        private readonly EmailSettings settings;

        public EmailComposer(
            IOptions<EmailSettings> settings)
        {
            this.settings = settings.Value;
        }

        public MimeMessage Compose(
            PasswordResetEmail email)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("mensch.ID", settings.FromAddress));
            message.To.Add(new MailboxAddress(email.RecipientAddress, email.RecipientAddress));
            message.Subject = Translate(PasswordResetEmailContent.Subject, email.PreferedLanguage);
            message.Body = new TextPart(TextFormat.Plain)
            {
                Text = Translate(PasswordResetEmailContent.Message, email.PreferedLanguage)
                    .Replace(
                        PasswordResetEmailContent.ResetLinkPlaceholder, 
                        $"https://mensch.id/reset-password?accountId={email.AccountId}&token={HttpUtility.UrlEncode(email.ResetToken)}")
            };
            return message;
        }

        public MimeMessage Compose(
            VerificationEmail email)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("mensch.ID", settings.FromAddress));
            message.To.Add(new MailboxAddress(email.RecipientAddress, email.RecipientAddress));
            message.Subject = Translate(VerificationEmailContent.Subject, email.PreferedLanguage);
            message.Body = new TextPart(TextFormat.Plain)
            {
                Text = Translate(VerificationEmailContent.Message, email.PreferedLanguage)
                    .Replace(
                        VerificationEmailContent.VerificationLinkPlaceholder, 
                        $"https://mensch.id/verify-email?accountId={email.AccountId}&token={HttpUtility.UrlEncode(email.VerificationToken)}")
            };
            return message;
        }

        private string Translate(
            IReadOnlyDictionary<Language, string> resourceDictionary,
            Language language)
        {
            if (resourceDictionary.ContainsKey(language))
                return resourceDictionary[language];
            return resourceDictionary[Language.en];
        }
    }
}
