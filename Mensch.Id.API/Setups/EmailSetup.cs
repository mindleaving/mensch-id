using Mensch.Id.API.Workflow.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mensch.Id.API.Setups
{
    public class EmailSetup : ISetup
    {
        public void Run(
            IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SettingsName));
            services.AddSingleton<EmailComposer>();
            if (configuration["Email:UseDummySender"]?.ToLower() == "true")
            {
                services.AddSingleton<IEmailSender, DummyEmailSender>();
            }
            else
            {
                services.AddSingleton<IEmailSender, EmailSender>();
            }
            
            

            services.AddHostedService<OrderDigestEmailSender>();
        }
    }
}
