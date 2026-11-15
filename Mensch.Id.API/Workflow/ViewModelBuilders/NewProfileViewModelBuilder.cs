using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Commons.Extensions;
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

        public async Task<NewProfileViewModel> BuildForAssigner(
            DateOnly birthDate,
            string accountId,
            int idCandidateCount = 1)
        {
            var birthDatePrefix = birthDate.ToString("yyyyMMdd");
            var reservedIds = idStore.SearchAsync(x => 
                x.ReservingAccountId == accountId
                && !x.IsClaimed
                && x.Id.StartsWith(birthDatePrefix)
                && x.Type == IdType.MenschID);
            var idCandidates = await reservedIds
                .Select(x => x.Id)
                .ToListAsync();
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
