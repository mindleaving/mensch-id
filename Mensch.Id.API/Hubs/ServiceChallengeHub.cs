using System;
using System.Threading.Tasks;
using Mensch.Id.API.AccessControl.Policies;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models.ExternalServices;
using Mensch.Id.API.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Mensch.Id.API.Hubs;

[Authorize(Policy = AccountTypeRequirement.ExternalServicePolicyName)]
public class ServiceChallengeHub : Hub
{
    public const string Route = "/hubs/service-challenges";
    public const string ChallengeAnsweredMethodName = "ChallengeAnswered";
    private readonly IReadonlyStore<ExternalService, Guid> serviceStore;

    public ServiceChallengeHub(IReadonlyStore<ExternalService, Guid> serviceStore)
    {
        this.serviceStore = serviceStore;
    }

    public static string GetGroupName(Guid serviceId)
    {
        return $"service-{serviceId}";
    }

    public override async Task OnConnectedAsync()
    {
        var serviceId = ClaimsHelpers.GetServiceId(Context.User!.Claims);
        var service = serviceId.HasValue
            ? await serviceStore.GetByIdAsync(serviceId.Value)
            : null;
        if (service == null || service.Status != ExternalServiceStatus.Active)
        {
            Context.Abort();
            return;
        }
        var groupName = GetGroupName(service.Id);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await base.OnConnectedAsync();
    }
}
