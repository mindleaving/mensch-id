using System;
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
        Task<List<LocalAnonymousAccount>> GetLocalsByMenschId(string menschId);
        Task<List<Account>> GetLocalsByEmailMenschIdOrUsernameAsync(string emailMenschIdOrUsername);
        Task<ExternalAccount> GetExternalByIdAsync(LoginProvider loginProvider, string externalId);
        Task<Account> GetFromClaimsAsync(List<Claim> claims);
        Task<List<PersonAccount>> GetAllForMenschIdAsync(string menschId);
        Task<ProfessionalAccount> GetProfessionalByUsernameAsync(string username);
        Task<StorageResult> ChangePasswordAsync(string accountId, string passwordHash);
        Task DeleteAllForPerson(string personId);
    }
}