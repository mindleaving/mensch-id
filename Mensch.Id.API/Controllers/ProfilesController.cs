﻿using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Commons.Extensions;
using Mensch.Id.API.AccessControl.Policies;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Storage;
using Mensch.Id.API.Workflow;
using Mensch.Id.API.Workflow.ViewModelBuilders;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mensch.Id.API.Controllers
{
    [Authorize(Policy = RegularUserPolicy.PolicyName)]
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
        private readonly IStore<AssignerControlledProfile> assignerControlledProfileStore;

        public ProfilesController(
            IAccountStore accountStore,
            IStore<Person> personStore,
            IHttpContextAccessor httpContextAccessor,
            NewProfileViewModelBuilder newProfileViewModelBuilder,
            IIdStore idStore,
            ProfileViewModelBuilder profileViewModelBuilder,
            IStore<Verification> verificationStore,
            IStore<AssignerControlledProfile> assignerControlledProfileStore)
        {
            this.accountStore = accountStore;
            this.personStore = personStore;
            this.httpContextAccessor = httpContextAccessor;
            this.newProfileViewModelBuilder = newProfileViewModelBuilder;
            this.idStore = idStore;
            this.profileViewModelBuilder = profileViewModelBuilder;
            this.verificationStore = verificationStore;
            this.assignerControlledProfileStore = assignerControlledProfileStore;
        }

        [HttpGet("me")]
        public async Task<IActionResult> MyProfile()
        {
            var claims = ControllerHelpers.GetClaims(httpContextAccessor);
            var personId = ClaimsHelpers.GetPersonId(claims);
            if (personId == null)
            {
                var account = await accountStore.GetFromClaimsAsync(claims);
                if (account is not PersonAccount personAccount)
                    return NotFound();
                if (personAccount.PersonId == null)
                    return NotFound();
                personId = personAccount.PersonId;
            }
            var profileData = await personStore.GetByIdAsync(personId);
            if (profileData == null)
                return NotFound();
            var vm = await profileViewModelBuilder.Build(profileData);
            return Ok(vm);
        }

        
        [HttpPost("claim/{id}")]
        public async Task<IActionResult> ClaimId([FromRoute] string id)
        {
            if (!ControllerInputSanitizer.ValidateAndSanitzeMenschId(id, out id))
                return BadRequest("Invalid mensch.ID");
            var claims = ControllerHelpers.GetClaims(httpContextAccessor);
            var account = (PersonAccount)await accountStore.GetFromClaimsAsync(claims);
            if (account.PersonId != null)
            {
                if (account.PersonId != id)
                    return StatusCode((int)HttpStatusCode.Forbidden, "ID in body doesn't match your ID");
            }
            else
            {
                if (!await idStore.TryClaimId(id, account.Id))
                    return StatusCode((int)HttpStatusCode.InternalServerError, $"ID '{id}' could not be claimed");
            }

            if (await personStore.ExistsAsync(id))
                return Ok();

            var profile = new Person(id, DateTime.UtcNow, account.Id);
            try
            {
                await personStore.StoreAsync(profile);
                if (account.PersonId == null)
                {
                    try
                    {
                        account.PersonId = id;
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
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
            await idStore.ReleaseReservations(account.Id);
            return Ok(profile);
        }

        [HttpPost("take-control")]
        public async Task<IActionResult> TakeControlOfId(
            [FromBody] TakeControlBody body)
        {
            if (!ControllerInputSanitizer.ValidateAndSanitzeMenschId(body.Id, out var personId))
                return BadRequest("Invalid mensch.ID");
            var owningAccounts = await accountStore.GetAllForMenschIdAsync(personId);
            if (owningAccounts.Any())
                return StatusCode((int)HttpStatusCode.Forbidden, "ID is controlled by another account already");
            var assignerControlledProfile = await assignerControlledProfileStore.GetByIdAsync(personId);
            if (assignerControlledProfile == null)
                return NotFound();
            if (body.OwnershipSecret != assignerControlledProfile.OwnershipSecret)
                return StatusCode((int)HttpStatusCode.Unauthorized, "Invalid ownership secret");
            var claims = ControllerHelpers.GetClaims(httpContextAccessor);
            var account = await accountStore.GetFromClaimsAsync(claims);
            if(account is not PersonAccount personAccount)
                return StatusCode((int)HttpStatusCode.Unauthorized, "Assigners and admins cannot take permanent control of profiles");
            if (personAccount.PersonId != null)
            {
                if (personAccount.PersonId == personId)
                    return Ok();
                return StatusCode((int)HttpStatusCode.Forbidden, "Your account is already associated with a profile");
            }

            var profile = await personStore.GetByIdAsync(personId);
            if (profile == null)
            {
                profile = new Person(personId, DateTime.UtcNow, personAccount.Id);
                await personStore.StoreAsync(profile);
            }
            personAccount.PersonId = personId;
            await accountStore.StoreAsync(personAccount);
            await assignerControlledProfileStore.DeleteAsync(personId);
            return Ok(profile);
        }



        [HttpGet("new")]
        public async Task<IActionResult> NewProfile([FromQuery] DateTime birthDate)
        {
            if (birthDate == default)
                return BadRequest("No birthdate specified, but it's required for generating ID candidates");
            var claims = ControllerHelpers.GetClaims(httpContextAccessor);
            var account = (PersonAccount)await accountStore.GetFromClaimsAsync(claims);
            if (account.PersonId != null)
            {
                var profileData = await personStore.GetByIdAsync(account.PersonId);
                if (profileData != null)
                    return StatusCode((int)HttpStatusCode.Forbidden, "You already have a profile");
            }
            var vm = await newProfileViewModelBuilder.BuildForNormalUser(birthDate, account.Id);
            return Ok(vm);
        }


        [HttpPost("{id}/verify")]
        public async Task<IActionResult> VerifyPerson([FromRoute] string id)
        {
            if (!ControllerInputSanitizer.ValidateAndSanitzeMenschId(id, out id))
                return BadRequest("Invalid mensch.ID");
            var targetProfile = await personStore.GetByIdAsync(id);
            if (targetProfile == null)
                return NotFound();
            var claims = ControllerHelpers.GetClaims(httpContextAccessor);
            var myAccount = await accountStore.GetFromClaimsAsync(claims);
            if (!await CanVerifyOthers(myAccount))
                return StatusCode((int)HttpStatusCode.Forbidden, "You must have a profile and be verified yourself to verify other profiles");
            if (myAccount is PersonAccount personAccount && personAccount.PersonId == id)
                return StatusCode((int)HttpStatusCode.Forbidden, "You cannot verify yourself :D");
            var verification = new Verification
            {
                Id = Guid.NewGuid().ToString(),
                PersonId = id,
                VerifierId = myAccount.Id,
                Timestamp = DateTime.UtcNow
            };
            await verificationStore.StoreAsync(verification);
            return Ok(verification);
        }

        private async Task<bool> CanVerifyOthers(
            Account myAccount)
        {
            if (myAccount is ProfessionalAccount)
                return true;
            if (myAccount is PersonAccount personAccount)
            {
                if (personAccount.PersonId == null)
                    return false;
                var myVerifications = await verificationStore.SearchAsync(x => x.PersonId == personAccount.PersonId).ToListAsync();
                return myVerifications.Count > 0;
            }
            return false;
        }
    }
}
