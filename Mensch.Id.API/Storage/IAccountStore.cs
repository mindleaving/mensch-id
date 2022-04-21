using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Mensch.Id.API.Models;

namespace Mensch.Id.API.Storage
{
    public interface IAccountStore : IStore<Account>
    {
        Task<LocalAccount> GetLocalByEmailAsync(string email);
        Task<ExternalAccount> GetExternalByIdAsync(LoginProvider loginProvider, string externalId);
        Task<Account> GetFromClaimsAsync(List<Claim> claims);
        Task<StorageResult> ChangePasswordAsync(string email, string passwordBase64);
        Task DeleteAllForPerson(string personId);
    }
}