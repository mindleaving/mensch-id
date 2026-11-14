using System;
using System.Threading.Tasks;
using Mensch.Id.API.Models.ExternalServices;
using Mensch.Id.API.ViewModels;
using Microsoft.AspNetCore.SignalR;

namespace Mensch.Id.API.Hubs;

public class ServiceChallengeNotifier
{
    private readonly IHubContext<ServiceChallengeHub> serviceHubContext;
    private readonly IHubContext<UserChallengeHub> userHubContext;

    public ServiceChallengeNotifier(
        IHubContext<ServiceChallengeHub> serviceHubContext,
        IHubContext<UserChallengeHub> userHubContext)
    {
        this.serviceHubContext = serviceHubContext;
        this.userHubContext = userHubContext;
    }

    public Task NotifyServiceAboutAnswer(
        Guid serviceId,
        ServiceChallengeResult result)
    {
        var groupName = ServiceChallengeHub.GetGroupName(serviceId);
        var clients = serviceHubContext.Clients.Group(groupName);
        return clients.SendAsync(ServiceChallengeHub.ChallengeAnsweredMethodName, result);
    }

    public Task NotifyUserAboutNewChallenge(
        string menschId,
        ServiceChallengeViewModel challenge)
    {
        var groupName = UserChallengeHub.GetGroupName(menschId);
        var clients = userHubContext.Clients.Group(groupName);
        return clients.SendAsync(UserChallengeHub.ChallengeCreatedMethodName, challenge);
    }
}
