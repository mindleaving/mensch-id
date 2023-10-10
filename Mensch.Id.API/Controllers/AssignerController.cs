using System;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
    private readonly IStore<AssignerControlledProfile> assignerControlledProfileStore;
    private readonly SignedProfileBuilder signedProfileBuilder;
    private readonly IFilterExpressionBuilder<AssignerControlledProfile,AssignedProfilesRequestParameters> filterExpressionBuilder;
    private readonly IFilesStore fileStore;

    public AssignerController(
        IStore<Account> accountStore,
        NewProfileViewModelBuilder newProfileViewModelBuilder,
        IHttpContextAccessor httpContextAccessor,
        IIdStore idStore,
        IStore<Person> profileStore,
        IStore<AssignerControlledProfile> assignerControlledProfileStore,
        SignedProfileBuilder signedProfileBuilder,
        IFilterExpressionBuilder<AssignerControlledProfile,AssignedProfilesRequestParameters> filterExpressionBuilder,
        IFilesStore fileStore)
    {
        this.accountStore = accountStore;
        this.newProfileViewModelBuilder = newProfileViewModelBuilder;
        this.httpContextAccessor = httpContextAccessor;
        this.idStore = idStore;
        this.profileStore = profileStore;
        this.assignerControlledProfileStore = assignerControlledProfileStore;
        this.signedProfileBuilder = signedProfileBuilder;
        this.filterExpressionBuilder = filterExpressionBuilder;
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
        var acount = await accountStore.GetByIdAsync(accountId);
        if (acount is not AssignerAccount assignerAccount)
            return NotFound();
        var stream = fileStore.GetById(logoId);
        return File(stream, contentType: assignerAccount.LogoImageType);
    }


    [HttpPut("me/logo")]
    public async Task<IActionResult> UploadLogo()
    {
        if (Request.ContentType == null)
            return BadRequest("No content type specified");
        if (!Regex.IsMatch(Request.ContentType, "^image/(png|gif|jpeg|svg\\+xml)$"))
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
        var person = new Person(idCandidate, DateTime.UtcNow, accountId);
        await profileStore.StoreAsync(person);
        return Ok(person);
    }

    [HttpGet("assigned-ids")]
    [HttpGet("assigned-ids/search")]
    public async Task<IActionResult> AssignedIds(
        [FromQuery] AssignedProfilesRequestParameters queryParameters)
    {
        var accountId = ControllerHelpers.GetAccountId(httpContextAccessor);
        var filterExpressions = filterExpressionBuilder.Build(queryParameters);
        filterExpressions.Add(x => x.AssignerAccountId == accountId);
        var combinedFilter = SearchExpressionBuilder.And(filterExpressions.ToArray());
        var orderByExpression = BuildOrderByExpression(queryParameters.OrderBy);
        var items = assignerControlledProfileStore.SearchAsync(
            combinedFilter,
            queryParameters.Count,
            queryParameters.Skip,
            orderByExpression,
            queryParameters.OrderDirection);
        return Ok(items);
    }

    private Expression<Func<AssignerControlledProfile, object>> BuildOrderByExpression(
        string orderBy)
    {
        return orderBy?.ToLower() switch
        {
            "time" => x => x.CreationDate,
            "id" => x => x.Id,
            _ => x => x.CreationDate
        };
    }

    [HttpGet("profiles/{personId}")]
    public async Task<IActionResult> GetAssignerControlledProfile(
        [FromRoute] string personId,
        [FromQuery] bool signed = false)
    {
        if (!ControllerInputSanitizer.ValidateAndSanitzeMenschId(personId, out personId))
            return BadRequest("Invalid person-ID");
        var profile = await assignerControlledProfileStore.GetByIdAsync(personId);
        if (profile == null)
            return NotFound();
        if(!signed)
            return Ok(profile);
        var signedProfile = signedProfileBuilder.Build(profile);
        return Ok(signedProfile);
    }

}