using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Mensch.Id.API.AccessControl.Policies;
using Mensch.Id.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Storage;
using Mensch.Id.API.Workflow;
using Mensch.Id.API.Workflow.Email;
using Mensch.Id.API.ViewModels;

namespace Mensch.Id.API.Controllers;

[Authorize(Policy = AccountTypeRequirement.AdminPolicyName)]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IStore<AssignerAccountRequest> accountRequestStore;
    private readonly IAccountCreator accountCreator;
    private readonly IEmailSender emailSender;
    private readonly IAccountStore accountStore;
    private readonly IAssignerControlledProfileStore assignerControlledProfileStore;
    private readonly ActiveSessionManager sessionManager;

    public AdminController(
        IStore<AssignerAccountRequest> accountRequestStore,
        IAccountCreator accountCreator,
        IEmailSender emailSender,
        IAccountStore accountStore,
        IAssignerControlledProfileStore assignerControlledProfileStore,
        ActiveSessionManager sessionManager)
    {
        this.accountRequestStore = accountRequestStore;
        this.accountCreator = accountCreator;
        this.emailSender = emailSender;
        this.accountStore = accountStore;
        this.assignerControlledProfileStore = assignerControlledProfileStore;
        this.sessionManager = sessionManager;
    }

    [AllowAnonymous]
    [HttpPost("assigner-requests")]
    public async Task<IActionResult> RequestAssignerAccount(
        [FromBody] AssignerAccountRequest body)
    {
        if (await accountRequestStore.ExistsAsync(body.Id))
            return StatusCode((int)HttpStatusCode.Conflict, "A request with the same ID already exists");
        var isEmailInUse = (await accountStore.GetLocalByEmailAsync(body.Email)) != null;
        if (isEmailInUse)
            return BadRequest("Email is already in use for another account");
        await accountRequestStore.StoreAsync(body);
        return Ok();
    }

    [HttpGet("assigner-requests")]
    public async Task<IActionResult> GetAssignerRequests(
        [FromQuery] string searchText = null)
    {
        var requests = await accountRequestStore.GetAllAsync().ToListAsync();
        var matchingRequests = requests
            .Where(x => MatchesSearchText(searchText, x.ContactPersonName, x.Email))
            .ToList();
        return Ok(matchingRequests);
    }

    [HttpGet("assigners")]
    public async Task<IActionResult> GetAssigners(
        [FromQuery] string searchText = null,
        [FromQuery] AssignerAccountStatus? status = null)
    {
        var assigners = await accountStore.GetAllAssignersAsync();
        var matchingAssigners = assigners
            .Where(x => MatchesStatus(x, status))
            .Where(x => MatchesSearchText(searchText, x.Name, x.Username, x.Contact?.Email))
            .OrderBy(x => x.Name)
            .ToList();
        var viewModels = new List<AdminAssignerViewModel>();
        foreach (var assigner in matchingAssigners)
        {
            var viewModel = await BuildAssignerViewModel(assigner);
            viewModels.Add(viewModel);
        }
        return Ok(viewModels);
    }

    [HttpGet("assigners/{id}")]
    public async Task<IActionResult> GetAssigner(
        [FromRoute] string id)
    {
        if (!ControllerInputSanitizer.ValidateAndSanitizeMandatoryId(id, out id))
            return BadRequest("Invalid account-ID");
        var account = await accountStore.GetByIdAsync(id);
        if (account is not AssignerAccount assigner)
            return NotFound();
        var viewModel = await BuildAssignerViewModel(assigner);
        return Ok(viewModel);
    }

    [HttpPost("assigners/{id}/deactivate")]
    public async Task<IActionResult> DeactivateAssigner(
        [FromRoute] string id)
    {
        if (!ControllerInputSanitizer.ValidateAndSanitizeMandatoryId(id, out id))
            return BadRequest("Invalid account-ID");
        var account = await accountStore.GetByIdAsync(id);
        if (account is not AssignerAccount assigner)
            return NotFound();
        assigner.IsActivated = false;
        await accountStore.StoreAsync(assigner);
        await sessionManager.RevokeAll([assigner.Id]);
        return Ok();
    }

    [HttpPost("assigners/{id}/activate")]
    public async Task<IActionResult> ActivateAssigner(
        [FromRoute] string id)
    {
        if (!ControllerInputSanitizer.ValidateAndSanitizeMandatoryId(id, out id))
            return BadRequest("Invalid account-ID");
        var account = await accountStore.GetByIdAsync(id);
        if (account is not AssignerAccount assigner)
            return NotFound();
        assigner.IsActivated = true;
        await accountStore.StoreAsync(assigner);
        return Ok();
    }

    private async Task<AdminAssignerViewModel> BuildAssignerViewModel(AssignerAccount assigner)
    {
        var assignedProfileCount = await assignerControlledProfileStore.CountForAssigner(assigner.Id);
        return new AdminAssignerViewModel
        {
            Id = assigner.Id,
            Username = assigner.Username,
            Name = assigner.Name,
            Contact = assigner.Contact,
            LogoId = assigner.LogoId,
            IsActivated = assigner.IsActivated,
            AssignedProfileCount = assignedProfileCount
        };
    }

    private static bool MatchesStatus(
        AssignerAccount assigner,
        AssignerAccountStatus? status)
    {
        return status switch
        {
            AssignerAccountStatus.Activated => assigner.IsActivated,
            AssignerAccountStatus.Deactivated => !assigner.IsActivated,
            _ => true
        };
    }

    private static bool MatchesSearchText(
        string searchText,
        params string[] values)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return true;
        var trimmedSearchText = searchText.Trim();
        return values.Any(x => x != null && x.Contains(trimmedSearchText, StringComparison.OrdinalIgnoreCase));
    }

    [HttpPost("assigner-requests/{id}/approve")]
    public async Task<IActionResult> ApproveAssignerRequest(
        [FromRoute] string id)
    {
        if (!ControllerInputSanitizer.ValidateAndSanitizeMandatoryId(id, out id))
            return BadRequest("Invalid request-ID");
        var request = await accountRequestStore.GetByIdAsync(id);
        if (request == null)
            return NotFound();
        var password = RandomNumberGenerator.GetString("abcdefghijkmnpqrstuvwxyzACDEFGHJKLMNPQRSTUVWXYZ2345679", 8);
        AssignerAccount account;
        try
        {
            account = await accountCreator.CreateAssigner(
                request.ContactPersonName, 
                request.Email,
                password);
        }
        catch (DuplicateNameException e)
        {
            return StatusCode((int)HttpStatusCode.Conflict, e.Message);
        }

        return Ok();

        // TODO: Implement mechanism for assigners to set their initial password. 
        //account.PasswordResetToken = PasswordReset.GenerateToken(account.EmailVerificationAndPasswordResetSalt, out var unencryptedToken);
        //await accountStore.StoreAsync(account);
        //try
        //{
        //    var email = new AssignerAccountRequestApprovedEmail
        //    {
        //        AccountId = account.Id,
        //        Name = request.ContactPersonName,
        //        RecipientAddress = request.Email,
        //        PreferedLanguage = Language.en,
        //        ResetToken = unencryptedToken
        //    };
        //    await emailSender.SendAssignerAccountApprovedEmail(email);
        //    await accountRequestStore.DeleteAsync(id);
        //    return Ok();
        //}
        //catch
        //{
        //    await accountStore.DeleteAsync(account.Id);
        //    return StatusCode((int)HttpStatusCode.InternalServerError, "Could not send email to applicant");
        //}
    }

    [HttpDelete("assigner-requests/{id}")]
    public async Task<IActionResult> DeleteAssignerRequest(
        [FromRoute] string id)
    {
        if (!ControllerInputSanitizer.ValidateAndSanitizeMandatoryId(id, out id))
            return BadRequest("Invalid request-ID");
        await accountRequestStore.DeleteAsync(id);
        return Ok();
    }


}