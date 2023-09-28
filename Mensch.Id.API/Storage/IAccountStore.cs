using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;

namespace Mensch.Id.API.Storage
{
    public interface IAccountStore : IStore<Account>
    {
        Task<LocalAccount> GetLocalByEmailAsync(string email);
        Task<List<LocalAnonymousAccount>> GetLocalsByMenschId(
            string menschId);
        Task<List<LocalAnonymousAccount>> GetLocalsByEmailOrMenschIdAsync(
            string emailOrMenschId);
        Task<ExternalAccount> GetExternalByIdAsync(LoginProvider loginProvider, string externalId);
        Task<Account> GetFromClaimsAsync(List<Claim> claims);
        Task<List<Account>> GeAllForMenschIdAsync(string menschId);
        Task<StorageResult> ChangePasswordAsync(string accountId, string passwordHash);
        Task DeleteAllForPerson(string personId);
    }
}