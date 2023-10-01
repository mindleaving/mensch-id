using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Commons.Extensions;
using Mensch.Id.API.Models.Shop;
using Mensch.Id.API.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Mensch.Id.API.Workflow.Email
{
    public class OrderDigestEmailSender : IHostedService, IDisposable
    {
        private readonly IEmailSender emailSender;
        private readonly IServiceProvider services;
        private Timer timer;

        public OrderDigestEmailSender(
            IEmailSender emailSender,
            IServiceProvider services)
        {
            this.emailSender = emailSender;
            this.services = services;
        }

        public async Task StartAsync(
            CancellationToken cancellationToken)
        {
            if (timer == null)
                timer = new(Send, null, TimeSpan.Zero, TimeSpan.FromHours(6));
            else
                timer.Change(TimeSpan.Zero, TimeSpan.FromHours(6));
        }

        public async Task StopAsync(
            CancellationToken cancellationToken)
        {
            timer?.Change(Timeout.Infinite, Timeout.Infinite);
            timer = null;
        }

        private async void Send(object state)
        {
            using var scope = services.CreateScope();
            var orderStore = scope.ServiceProvider.GetRequiredService<IReadonlyStore<Order>>();

            var orders = await orderStore
                .SearchAsync(x => x.Status == OrderStatus.Placed, orderBy: x => x.StatusChanges[0].Timestamp)
                .ToListAsync();
            if(orders.Count == 0)
                return;

            var orderDigestEmail = new OrderDigestEmail
            {
                OpenOrdersCount = orders.Count
            };
            try
            {
                await emailSender.SendOrderDigestEmail(orderDigestEmail);
            }
            catch
            {
                // Ignore
            }
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}
