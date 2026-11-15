using System;
using System.Threading.Tasks;
using Mensch.Id.API.Models.ExternalServices;
using Microsoft.AspNetCore.SignalR;

namespace Mensch.Id.API.Hubs;

public class ServiceChallengeNotifier
{
    private readonly IHubContext<ServiceChallengeHub> serviceHubContext;

    public ServiceChallengeNotifier(
        IHubContext<ServiceChallengeHub> serviceHubContext)
    {
        this.serviceHubContext = serviceHubContext;
    }

    public Task NotifyServiceAboutAnswer(
        Guid serviceId,
        ServiceChallengeResult result)
    {
        var groupName = ServiceChallengeHub.GetGroupName(serviceId);
        var clients = serviceHubContext.Clients.Group(groupName);
        return clients.SendAsync(ServiceChallengeHub.ChallengeAnsweredMethodName, result);
    }
}
