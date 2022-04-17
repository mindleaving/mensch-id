using System;
using System.Threading.Tasks;
using Mensch.Id.API.Models;
using Mensch.Id.API.Storage;
using Mensch.Id.API.Workflow;
using Microsoft.AspNetCore.Mvc;

namespace Mensch.Id.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IdController : ControllerBase
    {
        private readonly ChallengeCreator challengeCreator;
        private readonly IStore<MenschIdChallenge> challengeStore;
        private readonly IReadonlyStore<Person> personStore;

        public IdController(
            ChallengeCreator challengeCreator,
            IStore<MenschIdChallenge> challengeStore,
            IReadonlyStore<Person> personStore)
        {
            this.challengeCreator = challengeCreator;
            this.challengeStore = challengeStore;
            this.personStore = personStore;
        }

        [HttpGet("random")]
        public async Task<IActionResult> GetRandomId([FromQuery] DateTime birthdate)
        {
            var idGenerator = new MenschIdGenerator();
            var id = idGenerator.Generate(birthdate);
            return Ok(id);
        }

        [HttpGet("anonymous/random")]
        public async Task<IActionResult> GetAnonymousRandomId()
        {
            var idGenerator = new MenschIdGenerator();
            var id = idGenerator.GenerateAnonymous();
            return Ok(id);
        }


        [HttpPost("{menschId}/challenge")]
        public async Task<IActionResult> CreateChallenge([FromRoute] string menschId)
        {
            var challenge = await challengeCreator.Create(menschId);
            if (!await personStore.ExistsAsync(menschId))
                return Ok(challenge); // Return challenge, even if it isn't stored, to not give away which IDs are in use.
            await challengeStore.StoreAsync(challenge);
            return Ok(challenge);
        }

    }
}
