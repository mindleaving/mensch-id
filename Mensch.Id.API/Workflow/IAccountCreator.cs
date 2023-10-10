using System.Threading.Tasks;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;

namespace Mensch.Id.API.Workflow
{
    public interface IAccountCreator
    {
        Task<LocalAnonymousAccount> CreateLocalAnonymous(
            string password,
            Language preferedLanguage = Language.en,
            string menschId = null);

        Task<LocalAccount> CreateLocal(
            string email,
            string password,
            Language preferedLanguage = Language.en,
            string menschId = null);

        Task<ExternalAccount> CreateExternal(
            LoginProvider loginProvider,
            string externalId,
            Language preferedLanguage = Language.en,
            string menschId = null);

        Task<AssignerAccount> CreateAssigner(
            string name,
            string username,
            string password,
            Language preferedLanguage = Language.en);
    }
}