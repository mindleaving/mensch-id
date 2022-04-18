using System;
using System.Net;
using System.Threading.Tasks;
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
    [ApiController]
    [Route("api/[controller]")]
    public class ProfilesController : ControllerBase
    {
        private readonly IAccountStore accountStore;
        private readonly IStore<Person> personStore;
        private readonly IStore<Verification> verificationStore;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ProfileViewModelBuilder profileViewModelBuilder;
        private readonly NewProfileViewModelBuilder newProfileViewModelBuilder;
        private readonly IIdStore idStore;

        public ProfilesController(
            IAccountStore accountStore,
            IStore<Person> personStore,
            IHttpContextAccessor httpContextAccessor,
            NewProfileViewModelBuilder newProfileViewModelBuilder,
            IIdStore idStore,
            ProfileViewModelBuilder profileViewModelBuilder,
            IStore<Verification> verificationStore)
        {
            this.accountStore = accountStore;
            this.personStore = personStore;
            this.httpContextAccessor = httpContextAccessor;
            this.newProfileViewModelBuilder = newProfileViewModelBuilder;
            this.idStore = idStore;
            this.profileViewModelBuilder = profileViewModelBuilder;
            this.verificationStore = verificationStore;
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> MyProfile()
        {
            var claims = ControllerHelpers.GetClaims(httpContextAccessor);
            var account = await accountStore.GetFromClaimsAsync(claims);
            var profileData = await personStore.GetByIdAsync(account.PersonId);
            if (profileData == null)
                return NotFound();
            var vm = await profileViewModelBuilder.Build(profileData);
            return Ok(vm);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Store([FromRoute] string id, [FromBody] Person model)
        {
            if (!MenschIdGenerator.ValidateId(id))
                return BadRequest($"Invalid mensch.ID {id}");
            if (model.Id != id)
                return BadRequest("ID of route doesn't match ID in body");
            var claims = ControllerHelpers.GetClaims(httpContextAccessor);
            var account = await accountStore.GetFromClaimsAsync(claims);
            if (account.PersonId != null)
            {
                if (account.PersonId != id)
                    return StatusCode((int)HttpStatusCode.Forbidden, "ID in body doesn't match your ID");
                var existingProfile = await personStore.GetByIdAsync(account.PersonId);
                if (existingProfile != null)
                {
                    if (model.AnonymousId != existingProfile.AnonymousId)
                        return StatusCode((int)HttpStatusCode.Forbidden, "Anonymous ID in body doesn't match anonymous ID in your existing profile. You cannot change your IDs");
                }
            }
            else
            {
                if (!await idStore.TryClaimId(id, account.Id))
                    return StatusCode((int)HttpStatusCode.InternalServerError, $"ID '{id}' could not be claimed");
                if (!await idStore.TryClaimId(model.AnonymousId, account.Id))
                {
                    await idStore.UnclaimId(id);
                    return StatusCode((int)HttpStatusCode.InternalServerError, $"Anonymous ID '{model.AnonymousId}' could not be claimed");
                }
            }

            try
            {
                await personStore.StoreAsync(model);
                if (account.PersonId == null)
                {
                    try
                    {
                        account.PersonId = model.Id;
                        await accountStore.StoreAsync(account);
                    }
                    catch
                    {
                        await personStore.DeleteAsync(id);
                        throw;
                    }
                }
            }
            catch
            {
                await idStore.UnclaimId(id);
                await idStore.UnclaimId(model.AnonymousId);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
            await idStore.ReleaseReservations(account.Id);
            return Ok();
        }


        [Authorize]
        [HttpGet("new")]
        public async Task<IActionResult> NewProfile([FromQuery] DateTime birthDate)
        {
            if (birthDate == default)
                return BadRequest("No birthdate specified, but it's required for generating ID candidates");
            var claims = ControllerHelpers.GetClaims(httpContextAccessor);
            var account = await accountStore.GetFromClaimsAsync(claims);
            var profileData = await personStore.GetByIdAsync(account.PersonId);
            if (profileData != null)
                return StatusCode((int)HttpStatusCode.Forbidden, "You already have a profile");
            var vm = await newProfileViewModelBuilder.Build(birthDate, account.Id);
            return Ok(vm);
        }


        [Authorize]
        [HttpPost("{id}/verify")]
        public async Task<IActionResult> VerifyPerson([FromRoute] string id)
        {
            var targetProfile = await personStore.GetByIdAsync(id);
            if (targetProfile == null)
                return NotFound();
            var claims = ControllerHelpers.GetClaims(httpContextAccessor);
            var myAccount = await accountStore.GetFromClaimsAsync(claims);
            if (myAccount.PersonId == null)
                return StatusCode((int)HttpStatusCode.Forbidden, "You must have a profile and be verified yourself to verify other profiles");
            if (myAccount.PersonId == id)
                return StatusCode((int)HttpStatusCode.Forbidden, "You cannot verify yourself :D");
            var myVerifications = await verificationStore.SearchAsync(x => x.PersonId == myAccount.PersonId);
            if (myVerifications.Count == 0)
                return StatusCode((int)HttpStatusCode.Forbidden, "You must be verified yourself to verify other profiles");
            var verification = new Verification
            {
                Id = Guid.NewGuid().ToString(),
                PersonId = id,
                VerifierId = myAccount.PersonId,
                Timestamp = DateTime.UtcNow
            };
            await verificationStore.StoreAsync(verification);
            return Ok(verification);
        }

    }
}
