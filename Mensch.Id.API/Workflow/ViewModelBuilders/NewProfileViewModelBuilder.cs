using System;
using System.Linq;
using System.Threading.Tasks;
using Mensch.Id.API.Models;
using Mensch.Id.API.Storage;
using Mensch.Id.API.ViewModels;

namespace Mensch.Id.API.Workflow.ViewModelBuilders
{
    public class NewProfileViewModelBuilder
    {
        private const int IdCandidateCount = 10;
        private const int AnonymousIdCandidateCount = 10;

        private readonly IIdStore idStore;

        public NewProfileViewModelBuilder(IIdStore idStore)
        {
            this.idStore = idStore;
        }

        public async Task<NewProfileViewModel> Build(
            DateTime birthDate,
            string accountId)
        {
            var idGenerator = new MenschIdGenerator();
            await idStore.ReleaseReservationsNotMatchingBirthdate(accountId, birthDate);
            var reservedIds = await idStore.SearchAsync(x => x.ReservingAccountId == accountId);
            var idCandidates = reservedIds
                .Where(x => x.Type == IdType.MenschID && x.Id.StartsWith(birthDate.ToString("yyyyMMdd")))
                .Select(x => x.Id)
                .ToList();
            while (idCandidates.Count < IdCandidateCount)
            {
                var idCandidate = idGenerator.Generate(birthDate);
                if(!await idStore.TryReserveCandidate(idCandidate, IdType.MenschID, accountId))
                    continue;
                idCandidates.Add(idCandidate);
            }

            var anonymousIdCandidates = reservedIds.Where(x => x.Type == IdType.Anonymous).Select(x => x.Id).ToList();
            while (anonymousIdCandidates.Count < AnonymousIdCandidateCount)
            {
                var idCandidate = idGenerator.GenerateAnonymous();
                if(!await idStore.TryReserveCandidate(idCandidate, IdType.Anonymous, accountId))
                    continue;
                anonymousIdCandidates.Add(idCandidate);
            }

            return new NewProfileViewModel(idCandidates, anonymousIdCandidates);
        }
    }
}
