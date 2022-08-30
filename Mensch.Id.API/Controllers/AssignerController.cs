using System;
using System.Net;
using System.Threading.Tasks;
using Mensch.Id.API.AccessControl.Policies;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;
using Mensch.Id.API.Storage;
using Mensch.Id.API.Workflow;
using Mensch.Id.API.Workflow.ViewModelBuilders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mensch.Id.API.Controllers
{
    [Authorize(Policy = AssignerPolicy.PolicyName)]
    [ApiController]
    [Route("api/[controller]")]
    public class AssignerController : ControllerBase
    {
        private readonly IIdStore idStore;
        private readonly NewProfileViewModelBuilder newProfileViewModelBuilder;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IStore<Person> profileStore;
        private readonly IStore<AssignerControlledProfile> assignerControlledProfileStore;

        public AssignerController(
            NewProfileViewModelBuilder newProfileViewModelBuilder,
            IHttpContextAccessor httpContextAccessor,
            IIdStore idStore,
            IStore<Person> profileStore,
            IStore<AssignerControlledProfile> assignerControlledProfileStore)
        {
            this.newProfileViewModelBuilder = newProfileViewModelBuilder;
            this.httpContextAccessor = httpContextAccessor;
            this.idStore = idStore;
            this.profileStore = profileStore;
            this.assignerControlledProfileStore = assignerControlledProfileStore;
        }

        [HttpGet("candidate")]
        public async Task<IActionResult> GetIdCandidate(
            [FromQuery] DateTime birthDate)
        {
            if (birthDate == default)
                return BadRequest("No birthdate specified, but it's required for generating ID candidates");
            var accountId = ControllerHelpers.GetAccountId(httpContextAccessor);
            var vm = await newProfileViewModelBuilder.BuildForAssigner(birthDate, accountId);
            return Ok(vm);
        }

        [HttpPost("reject/{idCandidate}")]
        public async Task<IActionResult> RejectIdCandidate(
            [FromRoute] string idCandidate)
        {
            var accountId = ControllerHelpers.GetAccountId(httpContextAccessor);
            await idStore.ReleaseSpecificReservation(accountId, idCandidate);
            return Ok();
        }

        [HttpPost("claim/{idCandidate}")]
        public async Task<IActionResult> CreateNew(
            [FromRoute] string idCandidate)
        {
            var accountId = ControllerHelpers.GetAccountId(httpContextAccessor);
            if (!await idStore.TryClaimId(idCandidate, accountId))
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Could not claim ID '{idCandidate}'. Did you reserve it first?");
            var assignerControlledProfile = AssignerControlledProfileFactory.Create(idCandidate, accountId);
            await assignerControlledProfileStore.StoreAsync(assignerControlledProfile);
            var person = new Person(idCandidate, DateTime.UtcNow);
            await profileStore.StoreAsync(person);
            return Ok(person);
        }

        [HttpGet("assigned-ids")]
        public async Task<IActionResult> AssignedIds(
            [FromQuery] int? count = null,
            [FromQuery] int skip = 0)
        {
            var accountId = ControllerHelpers.GetAccountId(httpContextAccessor);
            var items = await assignerControlledProfileStore.SearchAsync(
                x => x.AssignerAccountId == accountId,
                count,
                skip,
                x => x.CreationDate,
                OrderDirection.Descending);
            return Ok(items);
        }

    }
}
