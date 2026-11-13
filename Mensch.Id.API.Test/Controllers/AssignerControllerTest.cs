using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Controllers;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Models.RequestParameters;
using Mensch.Id.API.Storage;
using Mensch.Id.API.Test.TestInfrastructure;
using Mensch.Id.API.Workflow;
using Mensch.Id.API.Workflow.FilterExpressionBuilders;
using Mensch.Id.API.Workflow.ViewModelBuilders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Moq;
using NUnit.Framework;

namespace Mensch.Id.API.Test.Controllers;

public class AssignerControllerTest
{
    private const string AssignerAccountId = "assigner-1";
    private static readonly DateOnly BirthDate = new(2000, 1, 1);
    private InMemoryAccountStore accountStore;
    private InMemoryIdStore idStore;
    private InMemoryStore<Person> personStore;
    private InMemoryStore<AssignerControlledProfile> assignerControlledProfileStore;
    private AssignerController controller;

    [SetUp]
    public async Task Setup()
    {
        accountStore = new InMemoryAccountStore(TestKeys.CreateObscurer());
        idStore = new InMemoryIdStore();
        personStore = new InMemoryStore<Person>();
        assignerControlledProfileStore = new InMemoryStore<AssignerControlledProfile>();
        await accountStore.StoreAsync(new AssignerAccount { Id = AssignerAccountId, Username = "clinic", Name = "Clinic" });

        var claims = new[]
        {
            new Claim(MenschIdClaimTypes.Subject, AssignerAccountId),
            new Claim(MenschIdClaimTypes.AccountTypeClaimName, nameof(AccountType.Assigner))
        };
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test")) };
        var httpContextAccessor = Mock.Of<IHttpContextAccessor>(x => x.HttpContext == httpContext);
        controller = new AssignerController(
            accountStore,
            new NewProfileViewModelBuilder(idStore),
            httpContextAccessor,
            idStore,
            personStore,
            assignerControlledProfileStore,
            null,
            Mock.Of<IFilterExpressionBuilder<AssignerControlledProfile, AssignedProfilesRequestParameters>>(),
            Mock.Of<IFilesStore>());
    }

    private async Task<string> ReserveCandidate()
    {
        var candidateResult = await controller.GetIdCandidate(BirthDate);
        var viewModel = (Mensch.Id.API.ViewModels.NewProfileViewModel)((OkObjectResult)candidateResult).Value!;
        return viewModel.IdCandidates.Single();
    }

    [Test]
    public async Task CandidateIsReservedForAssigner()
    {
        var idCandidate = await ReserveCandidate();

        var reservation = await idStore.GetByIdAsync(idCandidate);
        Assert.That(idCandidate, Does.StartWith("20000101-"));
        Assert.That(MenschIdGenerator.ValidateId(idCandidate), Is.True);
        Assert.That(reservation.ReservingAccountId, Is.EqualTo(AssignerAccountId));
        Assert.That(reservation.IsClaimed, Is.False);
    }

    [Test]
    public async Task ClaimingReservedCandidateCreatesPersonAndAssignerControlledProfile()
    {
        var idCandidate = await ReserveCandidate();

        var result = await controller.CreateNew(idCandidate);

        var person = await personStore.GetByIdAsync(idCandidate);
        var assignerControlledProfile = await assignerControlledProfileStore.GetByIdAsync(idCandidate);
        var reservation = await idStore.GetByIdAsync(idCandidate);
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        Assert.That(person, Is.Not.Null);
        Assert.That(person.CreationAccountId, Is.EqualTo(AssignerAccountId));
        Assert.That(assignerControlledProfile.AssignerAccountId, Is.EqualTo(AssignerAccountId));
        Assert.That(assignerControlledProfile.OwnershipSecret, Has.Length.EqualTo(AssignerControlledProfileFactory.OwnershipSecretLength));
        Assert.That(reservation.IsClaimed, Is.True);
    }

    [Test]
    public async Task ClaimingCandidateTwiceDoesNotReplaceOwnershipSecret()
    {
        var idCandidate = await ReserveCandidate();
        await controller.CreateNew(idCandidate);
        var originalSecret = (await assignerControlledProfileStore.GetByIdAsync(idCandidate)).OwnershipSecret;

        var result = await controller.CreateNew(idCandidate);

        var currentSecret = (await assignerControlledProfileStore.GetByIdAsync(idCandidate)).OwnershipSecret;
        Assert.That(((IStatusCodeActionResult)result).StatusCode, Is.EqualTo(409));
        Assert.That(currentSecret, Is.EqualTo(originalSecret));
    }

    [Test]
    public async Task ClaimingCandidateAfterOwnershipTransferDoesNotRecreateAssignerProfile()
    {
        var idCandidate = await ReserveCandidate();
        await controller.CreateNew(idCandidate);
        await assignerControlledProfileStore.DeleteAsync(idCandidate);

        var result = await controller.CreateNew(idCandidate);

        Assert.That(((IStatusCodeActionResult)result).StatusCode, Is.EqualTo(409));
        Assert.That(await assignerControlledProfileStore.ExistsAsync(idCandidate), Is.False);
    }

    [Test]
    public async Task ClaimingUnreservedCandidateFails()
    {
        var result = await controller.CreateNew("20000101-ACDEF");

        Assert.That(result, Is.InstanceOf<IStatusCodeActionResult>());
        Assert.That(((IStatusCodeActionResult)result).StatusCode, Is.EqualTo(500));
        Assert.That(personStore.Items, Is.Empty);
        Assert.That(assignerControlledProfileStore.Items, Is.Empty);
    }

    [Test]
    public async Task ClaimingCandidateReservedByOtherAccountFails()
    {
        await idStore.TryReserveCandidate("20000101-ACDEF", IdType.MenschID, "other-assigner");

        var result = await controller.CreateNew("20000101-ACDEF");

        Assert.That(((IStatusCodeActionResult)result).StatusCode, Is.EqualTo(500));
        Assert.That(personStore.Items, Is.Empty);
    }

    [Test]
    [TestCase("not-an-id")]
    [TestCase("x20000101-ACDEF")]
    public async Task InvalidCandidateIsRejected(string idCandidate)
    {
        var result = await controller.CreateNew(idCandidate);

        Assert.That(((IStatusCodeActionResult)result).StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task RejectedCandidateIsReleased()
    {
        var idCandidate = await ReserveCandidate();

        await controller.RejectIdCandidate(idCandidate);

        Assert.That(await idStore.ExistsAsync(idCandidate), Is.False);
    }
}
