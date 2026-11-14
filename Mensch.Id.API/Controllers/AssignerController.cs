using System;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Commons.DatabaseTools;
using Commons.Extensions;
using Mensch.Id.API.AccessControl.Policies;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Models.RequestParameters;
using Mensch.Id.API.Storage;
using Mensch.Id.API.ViewModels;
using Mensch.Id.API.Workflow;
using Mensch.Id.API.Workflow.FilterExpressionBuilders;
using Mensch.Id.API.Workflow.ViewModelBuilders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mensch.Id.API.Controllers;

[Authorize(Policy = AccountTypeRequirement.AssignerPolicyName)]
[ApiController]
[Route("api/[controller]")]
public class AssignerController : ControllerBase
{
    private readonly IStore<Account> accountStore;
    private readonly IIdStore idStore;
    private readonly NewProfileViewModelBuilder newProfileViewModelBuilder;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IStore<Person> profileStore;
    private readonly IAssignerControlledProfileStore assignerControlledProfileStore;
    private readonly SignedProfileBuilder signedProfileBuilder;
    private readonly IFilesStore fileStore;

    public AssignerController(
        IStore<Account> accountStore,
        NewProfileViewModelBuilder newProfileViewModelBuilder,
        IHttpContextAccessor httpContextAccessor,
        IIdStore idStore,
        IStore<Person> profileStore,
        IAssignerControlledProfileStore assignerControlledProfileStore,
        SignedProfileBuilder signedProfileBuilder,
        IFilesStore fileStore)
    {
        this.accountStore = accountStore;
        this.newProfileViewModelBuilder = newProfileViewModelBuilder;
        this.httpContextAccessor = httpContextAccessor;
        this.idStore = idStore;
        this.profileStore = profileStore;
        this.assignerControlledProfileStore = assignerControlledProfileStore;
        this.signedProfileBuilder = signedProfileBuilder;
        this.fileStore = fileStore;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyInformation()
    {
        var accountId = ControllerHelpers.GetAccountId(httpContextAccessor);
        var acount = await accountStore.GetByIdAsync(accountId);
        if (acount is not AssignerAccount assignerAccount)
            return NotFound();
        return Ok(new AssignerAccountViewModel(assignerAccount));
    }

    [HttpPut("me/name")]
    public async Task<IActionResult> SetName(
        [FromBody] string name)
    {
        var accountId = ControllerHelpers.GetAccountId(httpContextAccessor);
        var acount = await accountStore.GetByIdAsync(accountId);
        if (acount is not AssignerAccount assignerAccount)
            return NotFound();
        assignerAccount.Name = name;
        await accountStore.StoreAsync(assignerAccount);
        return Ok(new AssignerAccountViewModel(assignerAccount));
    }

    [HttpPut("me/contact")]
    public async Task<IActionResult> SetContact(
        [FromBody] Contact body)
    {
        var accountId = ControllerHelpers.GetAccountId(httpContextAccessor);
        var acount = await accountStore.GetByIdAsync(accountId);
        if (acount is not AssignerAccount assignerAccount)
            return NotFound();
        assignerAccount.Contact = body;
        await accountStore.StoreAsync(assignerAccount);
        return Ok(new AssignerAccountViewModel(assignerAccount));
    }

    [AllowAnonymous]
    [HttpGet("{accountId}/logo/{logoId}")]
    public async Task<IActionResult> GetLogo(
        [FromRoute] string accountId,
        [FromRoute] string logoId)
    {
        if (!ControllerInputSanitizer.ValidateAndSanitizeMandatoryId(accountId, out accountId))
            return BadRequest("Invalid account-ID");
        var acount = await accountStore.GetByIdAsync(accountId);
        if (acount is not AssignerAccount assignerAccount)
            return NotFound();
        if (assignerAccount.LogoId == null || logoId != assignerAccount.LogoId)
            return NotFound();
        if (!IsAllowedLogoContentType(assignerAccount.LogoImageType))
            return NotFound();
        var stream = fileStore.GetById(logoId);
        return File(stream, contentType: assignerAccount.LogoImageType);
    }


    [HttpPut("me/logo")]
    public async Task<IActionResult> UploadLogo()
    {
        if (Request.ContentType == null)
            return BadRequest("No content type specified");
        if (!IsAllowedLogoContentType(Request.ContentType))
            return BadRequest("Unsupported image file type");
        var accountId = ControllerHelpers.GetAccountId(httpContextAccessor);
        var acount = await accountStore.GetByIdAsync(accountId);
        if (acount is not AssignerAccount assignerAccount)
            return NotFound();
        var oldLogoId = assignerAccount.LogoId;
        var logoFileId = Guid.NewGuid().ToString();
        await fileStore.StoreAsync(logoFileId, Request.Body);
        assignerAccount.LogoId = logoFileId;
        assignerAccount.LogoImageType = Request.ContentType;
        await accountStore.StoreAsync(assignerAccount);
        if(oldLogoId != null)
            fileStore.Delete(oldLogoId);
        return Ok();
    }

    [HttpDelete("me/logo")]
    public async Task<IActionResult> ClearLogo()
    {
        var accountId = ControllerHelpers.GetAccountId(httpContextAccessor);
        var acount = await accountStore.GetByIdAsync(accountId);
        if (acount is not AssignerAccount assignerAccount)
            return NotFound();
        try
        {
            fileStore.Delete(assignerAccount.LogoId);
        }
        catch (FileNotFoundException)
        {
            // Ignore
        }
        assignerAccount.LogoId = null;
        assignerAccount.LogoImageType = null;
        await accountStore.StoreAsync(assignerAccount);
        return Ok();
    }

    [HttpGet("candidate")]
    public async Task<IActionResult> GetIdCandidate(
        [FromQuery] DateOnly birthDate)
    {
        if (birthDate == default)
            return BadRequest("No birthdate specified, but it's required for generating ID candidates");
        var accountId = ControllerHelpers.GetAccountId(httpContextAccessor);
        var vm = await newProfileViewModelBuilder.BuildForAssigner(birthDate, accountId);
        return Ok(vm);
    }

    [HttpPost("reject/{idCandidate}")]
    public async Task<IActionResult> RejectIdCandidate(
        [FromRoute] MenschId idCandidate)
    {
        var accountId = ControllerHelpers.GetAccountId(httpContextAccessor);
        await idStore.ReleaseSpecificReservation(accountId, idCandidate.Value);
        return Ok();
    }

    [HttpPost("claim/{idCandidate}")]
    public async Task<IActionResult> CreateNew(
        [FromRoute] MenschId idCandidate)
    {
        var menschId = idCandidate.Value;
        var accountId = ControllerHelpers.GetAccountId(httpContextAccessor);
        if (await profileStore.ExistsAsync(menschId))
            return Conflict($"ID '{menschId}' has already been assigned");
        if (!await idStore.TryClaimId(menschId, accountId))
            return StatusCode((int)HttpStatusCode.InternalServerError, $"Could not claim ID '{menschId}'. Did you reserve it first?");
        var assignerControlledProfile = AssignerControlledProfileFactory.Create(idCandidate, accountId);
        await assignerControlledProfileStore.StoreAsync(assignerControlledProfile);
        var person = new Person(menschId, DateTime.UtcNow, accountId);
        await profileStore.StoreAsync(person);
        return Ok(person);
    }

    [HttpGet("assigned-ids")]
    [HttpGet("assigned-ids/search")]
    public async Task<IActionResult> AssignedIds(
        [FromQuery] AssignedProfilesRequestParameters queryParameters)
    {
        var accountId = ControllerHelpers.GetAccountId(httpContextAccessor);
        var items = assignerControlledProfileStore.SearchAssignedProfiles(accountId, queryParameters);
        return Ok(items);
    }

    [HttpGet("profiles/{personId}")]
    public async Task<IActionResult> GetAssignerControlledProfile(
        [FromRoute] MenschId personId,
        [FromQuery] bool signed = false)
    {
        var accountId = ControllerHelpers.GetAccountId(httpContextAccessor);
        var profile = await assignerControlledProfileStore.GetByIdAsync(personId);
        if (profile == null || profile.AssignerAccountId != accountId)
            return NotFound();
        if(!signed)
            return Ok(profile);
        var signedProfile = signedProfileBuilder.Build(profile);
        return Ok(signedProfile);
    }

    private static bool IsAllowedLogoContentType(string contentType)
    {
        return contentType != null && Regex.IsMatch(contentType, "^image/(png|gif|jpeg)$");
    }

}