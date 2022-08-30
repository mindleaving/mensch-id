using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mensch.Id.API.Models;
using Mensch.Id.API.Storage;
using Mensch.Id.API.ViewModels;

namespace Mensch.Id.API.Workflow.ViewModelBuilders
{
    public class NewProfileViewModelBuilder
    {
        private readonly IIdStore idStore;
        private readonly MenschIdGenerator idGenerator = new();

        public NewProfileViewModelBuilder(IIdStore idStore)
        {
            this.idStore = idStore;
        }

        public async Task<NewProfileViewModel> BuildForNormalUser(
            DateTime birthDate,
            string accountId,
            int idCandidateCount = 10)
        {
            await idStore.ReleaseReservationsNotMatchingBirthdate(accountId, birthDate);
            var reservedIds = await idStore.SearchAsync(x => x.ReservingAccountId == accountId);
            var claimedId = reservedIds.SingleOrDefault(x => x.IsClaimed);
            if(claimedId != null)
                return new NewProfileViewModel(new List<string>{ claimedId.Id });
            var idCandidates = reservedIds
                .Where(x => x.Type == IdType.MenschID && x.Id.StartsWith(birthDate.ToString("yyyyMMdd")))
                .Select(x => x.Id)
                .ToList();
            while (idCandidates.Count < idCandidateCount)
            {
                var idCandidate = idGenerator.Generate(birthDate);
                if(!await idStore.TryReserveCandidate(idCandidate, IdType.MenschID, accountId))
                    continue;
                idCandidates.Add(idCandidate);
            }

            return new NewProfileViewModel(idCandidates);
        }

        public async Task<NewProfileViewModel> BuildForAssigner(
            DateTime birthDate,
            string accountId,
            int idCandidateCount = 1)
        {
            var birthDatePrefix = birthDate.ToString("yyyyMMdd");
            var reservedIds = await idStore.SearchAsync(x => 
                x.ReservingAccountId == accountId
                && !x.IsClaimed
                && x.Id.StartsWith(birthDatePrefix)
                && x.Type == IdType.MenschID);
            var idCandidates = reservedIds
                .Select(x => x.Id)
                .ToList();
            while (idCandidates.Count < idCandidateCount)
            {
                var idCandidate = idGenerator.Generate(birthDate);
                if(!await idStore.TryReserveCandidate(idCandidate, IdType.MenschID, accountId))
                    continue;
                idCandidates.Add(idCandidate);
            }

            return new NewProfileViewModel(idCandidates);
        }
    }
}
