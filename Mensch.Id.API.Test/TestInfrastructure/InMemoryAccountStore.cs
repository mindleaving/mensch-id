using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Storage;
using Mensch.Id.API.Workflow;

namespace Mensch.Id.API.Test.TestInfrastructure;

public class InMemoryAccountStore : InMemoryStore<Account>, IAccountStore
{
    private readonly IExternalLoginObscurer externalLoginObscurer;

    public InMemoryAccountStore(IExternalLoginObscurer externalLoginObscurer)
    {
        this.externalLoginObscurer = externalLoginObscurer;
    }

    public Task<LocalAccount> GetLocalByEmailAsync(string email)
    {
        var account = items.Values.OfType<LocalAccount>().FirstOrDefault(x => x.Email == email);
        return Task.FromResult(account);
    }

    public Task<List<LocalAnonymousAccount>> GetLocalsByMenschId(string menschId)
    {
        var accounts = items.Values.OfType<LocalAnonymousAccount>().Where(x => x.PersonId == menschId).ToList();
        return Task.FromResult(accounts);
    }

    public async Task<List<Account>> GetLocalsByEmailMenschIdOrUsernameAsync(string emailMenschIdOrUsername)
    {
        if (MenschIdGenerator.ValidateId(emailMenschIdOrUsername))
        {
            var localAccounts = await GetLocalsByMenschId(emailMenschIdOrUsername);
            return localAccounts.Cast<Account>().ToList();
        }
        var localAccount = await GetLocalByEmailAsync(emailMenschIdOrUsername);
        if (localAccount != null)
            return [localAccount];
        var professionalAccount = await GetProfessionalByUsernameAsync(emailMenschIdOrUsername);
        if (professionalAccount != null)
            return [professionalAccount];
        return [];
    }

    public Task<ExternalAccount> GetExternalByIdAsync(
        LoginProvider loginProvider,
        string externalId)
    {
        var obscuredExternalId = externalLoginObscurer.Obscure(loginProvider, externalId);
        var account = items.Values
            .OfType<ExternalAccount>()
            .FirstOrDefault(x => x.LoginProvider == loginProvider && x.ExternalId == obscuredExternalId);
        return Task.FromResult(account);
    }

    public async Task<Account> GetFromClaimsAsync(List<Claim> claims)
    {
        var accountId = ClaimsHelpers.GetAccountId(claims);
        if (accountId != null)
            return await GetByIdAsync(accountId);
        var loginProvider = ClaimsHelpers.GetExternalLoginProvider(claims);
        if (loginProvider == null)
            return null;
        var externalId = ClaimsHelpers.GetExternalId(claims, loginProvider.Value);
        return await GetExternalByIdAsync(loginProvider.Value, externalId);
    }

    public Task<List<PersonAccount>> GetAllForMenschIdAsync(string menschId)
    {
        var accounts = items.Values.OfType<PersonAccount>().Where(x => x.PersonId == menschId).ToList();
        return Task.FromResult(accounts);
    }

    public Task<ProfessionalAccount> GetProfessionalByUsernameAsync(string username)
    {
        var account = items.Values.OfType<ProfessionalAccount>().FirstOrDefault(x => x.Username == username);
        return Task.FromResult(account);
    }

    public Task<StorageResult> ChangePasswordAsync(
        string accountId,
        string passwordHash)
    {
        if (!items.TryGetValue(accountId, out var account) || account is not IAccountWithPassword accountWithPassword)
            return Task.FromResult(StorageResult.Error(StoreErrorType.NoMatch));
        accountWithPassword.PasswordHash = passwordHash;
        if (account is LocalAccount localAccount)
            localAccount.PasswordResetNonce = null;
        return Task.FromResult(StorageResult.Success());
    }

    public Task DeleteAllForPerson(string personId)
    {
        var accountIds = items.Values
            .OfType<PersonAccount>()
            .Where(x => x.PersonId == personId)
            .Select(x => x.Id)
            .ToList();
        foreach (var accountId in accountIds)
        {
            items.TryRemove(accountId, out _);
        }
        return Task.CompletedTask;
    }
}
