using System.Threading.Tasks;
using Mensch.Id.API.AccessControl.Policies;
using Mensch.Id.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Mensch.Id.API.Hubs;

[Authorize(Policy = RegularUserPolicy.PolicyName)]
public class UserChallengeHub : Hub
{
    public const string Route = "/hubs/user-challenges";
    public const string ChallengeCreatedMethodName = "ChallengeCreated";

    public static string GetGroupName(string menschId)
    {
        return $"person-{menschId}";
    }

    public override async Task OnConnectedAsync()
    {
        var menschId = ClaimsHelpers.GetPersonId(Context.User!.Claims);
        if (string.IsNullOrEmpty(menschId))
        {
            Context.Abort();
            return;
        }
        var groupName = GetGroupName(menschId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await base.OnConnectedAsync();
    }
}
