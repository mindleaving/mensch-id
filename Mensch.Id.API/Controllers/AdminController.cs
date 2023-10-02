using Mensch.Id.API.AccessControl.Policies;
using Mensch.Id.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Storage;
using Mensch.Id.API.Workflow;
using Mensch.Id.API.Workflow.Email;

namespace Mensch.Id.API.Controllers;

[Authorize(Policy = AccountTypeRequirement.AdminPolicyName)]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IStore<AssignerAccountRequest> accountRequestStore;
    private readonly IAccountCreator accountCreator;
    private readonly IEmailSender emailSender;
    private readonly IStore<Account> accountStore;

    public AdminController(
        IStore<AssignerAccountRequest> accountRequestStore,
        IAccountCreator accountCreator,
        IEmailSender emailSender,
        IStore<Account> accountStore)
    {
        this.accountRequestStore = accountRequestStore;
        this.accountCreator = accountCreator;
        this.emailSender = emailSender;
        this.accountStore = accountStore;
    }

    [AllowAnonymous]
    [HttpPost("assigner-requests")]
    public async Task<IActionResult> RequestAssignerAccount(
        [FromBody] AssignerAccountRequest body)
    {
        if (await accountRequestStore.ExistsAsync(body.Id))
            return StatusCode((int)HttpStatusCode.Conflict, "A request with the same ID already exists");
        await accountRequestStore.StoreAsync(body);
        return Ok();
    }

    [HttpGet("assigner-requests")]
    public async Task<IActionResult> GetAssignerRequests()
    {
        var items = accountRequestStore.GetAllAsync();
        return Ok(items);
    }

    [HttpPost("assigner-requests/{id}/approve")]
    public async Task<IActionResult> ApproveAssignerRequest(
        [FromRoute] string id)
    {
        var request = await accountRequestStore.GetByIdAsync(id);
        if (request == null)
            return NotFound();
        var password = new TemporaryPasswordGenerator { AllowedCharacters = "abcdefghijkmnpqrstuvwxyzACDEFGHJKLMNPQRSTUVWXYZ2345679"}.Generate(8);
        var account = await accountCreator.CreateAssigner(
            request.ContactPersonName, 
            request.Email,
            password);
        account.PasswordResetToken = PasswordReset.GenerateToken(account.EmailVerificationAndPasswordResetSalt, out var unencryptedToken);
        await accountStore.StoreAsync(account);
        var email = new AssignerAccountRequestApprovedEmail
        {
            AccountId = account.Id,
            Name = request.ContactPersonName,
            RecipientAddress = request.Email,
            PreferedLanguage = Language.en,
            ResetToken = unencryptedToken
        };
        await emailSender.SendAssignerAccountApprovedEmail(email);
        return Ok();
    }

    [HttpDelete("assigner-requests/{id}")]
    public async Task<IActionResult> DeleteAssignerRequest(
        [FromRoute] string id)
    {
        await accountRequestStore.DeleteAsync(id);
        return Ok();
    }


}