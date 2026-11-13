using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Commons.Extensions;
using Mensch.Id.API.AccessControl;
using IAuthenticationModule = Mensch.Id.API.AccessControl.IAuthenticationModule;
using Mensch.Id.API.AccessControl.Policies;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Storage;
using Mensch.Id.API.Workflow;
using Mensch.Id.API.Workflow.Models;
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
        private readonly OwnershipTransfer ownershipTransfer;
        private readonly IAuthenticationModule authenticationModule;

        public ProfilesController(
            IAccountStore accountStore,
            IStore<Person> personStore,
            IHttpContextAccessor httpContextAccessor,
            ProfileViewModelBuilder profileViewModelBuilder,
            IStore<Verification> verificationStore,
            OwnershipTransfer ownershipTransfer,
            IAuthenticationModule authenticationModule)
        {
            this.authenticationModule = authenticationModule;
            this.accountStore = accountStore;
            this.personStore = personStore;
            this.httpContextAccessor = httpContextAccessor;
            this.profileViewModelBuilder = profileViewModelBuilder;
            this.verificationStore = verificationStore;
            this.ownershipTransfer = ownershipTransfer;
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

        
        [HttpPost("take-control")]
        public async Task<IActionResult> TakeControlOfId(
            [FromBody] TakeControlBody body)
        {
            if (!ControllerInputSanitizer.ValidateAndSanitzeMenschId(body.Id, out var personId))
                return BadRequest("Invalid mensch.ID");
            var claims = ControllerHelpers.GetClaims(httpContextAccessor);
            var account = await accountStore.GetFromClaimsAsync(claims);
            var result = await ownershipTransfer.TakeControl(
                account,
                personId,
                body.OwnershipSecret);
            if (result.IsSuccess)
                await RefreshSessionClaims(account);
            return result.Status switch
            {
                OwnershipTransferStatus.Success => Ok(result.Profile),
                OwnershipTransferStatus.ProfileNotFound => NotFound(),
                OwnershipTransferStatus.InvalidOwnershipSecret => StatusCode((int)HttpStatusCode.Unauthorized, "Invalid ownership secret"),
                OwnershipTransferStatus.ControlledByOtherAccount => StatusCode((int)HttpStatusCode.Forbidden, "ID is controlled by another account already"),
                OwnershipTransferStatus.AccountNotEligible => StatusCode((int)HttpStatusCode.Forbidden, "Assigners and admins cannot take permanent control of profiles"),
                OwnershipTransferStatus.AccountAlreadyHasProfile => StatusCode((int)HttpStatusCode.Forbidden, "Your account is already associated with a profile"),
                _ => throw new ArgumentOutOfRangeException(nameof(result.Status), result.Status, null)
            };
        }


        private async Task RefreshSessionClaims(Account account)
        {
            var isSessionAuthenticated = User.Identities.Any(
                x => x.IsAuthenticated && x.AuthenticationType == MenschIdAuthenticationSchemes.Session);
            if (!isSessionAuthenticated)
                return;
            var authenticationResult = authenticationModule.BuildAuthenticationResult(account);
            await HttpContext.SignInAsync(MenschIdAuthenticationSchemes.Session, authenticationResult.Principal);
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
