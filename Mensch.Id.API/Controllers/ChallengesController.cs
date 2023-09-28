using System.Net;
using System.Threading.Tasks;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;
using Mensch.Id.API.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mensch.Id.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChallengesController : ControllerBase
    {
        private readonly IStore<MenschIdChallenge> challengeStore;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IAccountStore accountStore;
        private readonly IReadonlyStore<Person> personStore;

        public ChallengesController(
            IStore<MenschIdChallenge> challengeStore,
            IHttpContextAccessor httpContextAccessor,
            IAccountStore accountStore,
            IReadonlyStore<Person> personStore)
        {
            this.challengeStore = challengeStore;
            this.httpContextAccessor = httpContextAccessor;
            this.accountStore = accountStore;
            this.personStore = personStore;
        }

        [HttpGet("{challengeId}")]
        public async Task<IActionResult> GetChallengeById(string challengeId)
        {
            var claims = ControllerHelpers.GetClaims(httpContextAccessor);
            var account = await accountStore.GetFromClaimsAsync(claims);
            if (account.PersonId == null)
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, "You cannot see challenges yet because you haven't finished you profile yet");
            var matchingChallenge = await challengeStore.GetByIdAsync(challengeId);
            if (matchingChallenge == null)
            {
                matchingChallenge = await challengeStore.FirstOrDefaultAsync(x => x.MenschId == account.PersonId && x.ChallengeShortId == challengeId);
                if (matchingChallenge == null)
                    return NotFound();
            }

            return Ok(matchingChallenge);
        }

        [HttpGet]
        public async Task<IActionResult> Search(
            [FromQuery] string searchText,
            [FromQuery] int? count = null,
            [FromQuery] int? skip = 0)
        {
            var claims = ControllerHelpers.GetClaims(httpContextAccessor);
            var account = await accountStore.GetFromClaimsAsync(claims);
            if (account.PersonId == null)
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, "You cannot see challenges yet because you haven't finished you profile yet");
            var upperSearchText = searchText.ToUpper();
            var items = challengeStore.SearchAsync(x => 
                    x.MenschId == account.PersonId 
                    && (x.ChallengeShortId.StartsWith(upperSearchText) || x.Id.ToUpper().StartsWith(upperSearchText)),
                count,
                skip,
                x => x.CreatedTimestamp,
                OrderDirection.Descending);
            return Ok(items);
        }


        [HttpDelete("{challengeId}")]
        public async Task<IActionResult> DeleteChallenge([FromRoute] string challengeId)
        {
            var claims = ControllerHelpers.GetClaims(httpContextAccessor);
            var account = await accountStore.GetFromClaimsAsync(claims);
            if (account.PersonId == null)
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, "You cannot delete challenges yet because you haven't finished you profile yet");
            var matchingChallenge = await challengeStore.GetByIdAsync(challengeId);
            if (matchingChallenge == null)
                return Ok();
            if (matchingChallenge.MenschId != account.PersonId)
                return NotFound();
            await challengeStore.DeleteAsync(challengeId);
            return Ok();
        }


    }
}
