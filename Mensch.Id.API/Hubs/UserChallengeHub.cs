using System.Linq;
using System.Threading.Tasks;
using Mensch.Id.API.AccessControl.Policies;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Mensch.Id.API.Hubs;

[Authorize(Policy = RegularUserPolicy.PolicyName)]
public class UserChallengeHub : Hub
{
    public const string Route = "/hubs/user-challenges";
    public const string ChallengeCreatedMethodName = "ChallengeCreated";
    private readonly IAccountStore accountStore;

    public UserChallengeHub(IAccountStore accountStore)
    {
        this.accountStore = accountStore;
    }

    public static string GetGroupName(string menschId)
    {
        return $"person-{menschId}";
    }

    public override async Task OnConnectedAsync()
    {
        var claims = Context.User!.Claims.ToList();
        var account = await accountStore.GetFromClaimsAsync(claims);
        var menschId = (account as PersonAccount)?.PersonId;
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
