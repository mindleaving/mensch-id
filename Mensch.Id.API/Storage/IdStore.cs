using System;
using System.Threading.Tasks;
using Mensch.Id.API.Models;
using MongoDB.Driver;

namespace Mensch.Id.API.Storage
{
    public class IdStore : GenericStore<MenschId>, IIdStore
    {
        public IdStore(
            IMongoDatabase mongoDatabase,
            string collectionName = null)
            : base(mongoDatabase, collectionName)
        {
        }

        public async Task<bool> TryReserveCandidate(
            string idCandidate,
            IdType idType,
            string accountId)
        {
            var idObject = new MenschId
            {
                Id = idCandidate,
                Type = idType,
                ReservingAccountId = accountId
            };
            try
            {
                await collection.InsertOneAsync(idObject);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> TryClaimId(
            string id,
            string accountId)
        {
            var result = await collection.UpdateOneAsync(
                x => x.Id == id && x.ReservingAccountId == accountId,
                Builders<MenschId>.Update.Set(x => x.IsClaimed, true));
            return result.IsAcknowledged && result.MatchedCount == 1;
        }

        public Task UnclaimId(string id)
        {
            return collection.UpdateOneAsync(
                x => x.Id == id,
                Builders<MenschId>.Update.Set(x => x.IsClaimed, false));
        }

        public Task ReleaseSpecificReservation(
            string accountId,
            string idCandidate)
        {
            return collection.DeleteOneAsync(x => !x.IsClaimed && x.ReservingAccountId == accountId && x.Id == idCandidate);
        }

        public Task ReleaseReservations(string accountId)
        {
            return collection.DeleteManyAsync(x => !x.IsClaimed && x.ReservingAccountId == accountId);
        }

        public Task ReleaseReservationsNotMatchingBirthdate(
            string accountId,
            DateTime birthDate)
        {
            var prefix = birthDate.ToString("yyyyMMdd");
            return collection.DeleteManyAsync(x => 
                !x.IsClaimed
                && x.ReservingAccountId == accountId
                && x.Type == IdType.MenschID
                && !x.Id.StartsWith(prefix));
        }
    }
}
