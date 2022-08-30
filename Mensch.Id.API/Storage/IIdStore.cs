using System;
using System.Threading.Tasks;
using Mensch.Id.API.Models;

namespace Mensch.Id.API.Storage
{
    public interface IIdStore : IStore<MenschId>
    {
        Task<bool> TryReserveCandidate(string idCandidate, IdType idType, string accountId);
        Task<bool> TryClaimId(string id, string accountId);
        Task UnclaimId(string id);
        Task ReleaseSpecificReservation(string accountId, string idCandidate);
        Task ReleaseReservations(string accountId);
        Task ReleaseReservationsNotMatchingBirthdate(string accountId, DateTime birthDate);

    }
}
