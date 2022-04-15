using System.Threading.Tasks;
using Mensch.Id.API.Models;

namespace Mensch.Id.API.Workflow
{
    public interface IProfileCreator
    {
        Task<LocalAccount> CreateLocal(
            string username,
            string password,
            Language preferedLanguage = Language.en,
            string personId = null);
        Task<ExternalAccount> CreateExternal(
            LoginProvider loginProvider,
            string externalId,
            Language preferedLanguage = Language.en,
            string personId = null);
    }
}