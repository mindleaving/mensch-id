using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Commons;
using Mensch.Id.API.Models;
using Mensch.Id.API.Storage;
using Mensch.Id.API.Workflow;
using MongoDB.Driver;
using NUnit.Framework;

namespace Mensch.Id.API.Tools
{
    public class MenschIdGeneratorTest : DatabaseAccess
    {
        private static readonly DateTime BirthDate = new(1989, 11, 19);
        private static readonly string AccountId = "6f00b97a-0ef8-4b9e-8bf8-54350346bf93";

        [SetUp]
        [TearDown]
        public void ClearTestIdsFromDatabase()
        {
            var collection = GetCollection<MenschId>();
            collection.DeleteMany(Builders<MenschId>.Filter.Where(x => x.ReservingAccountId == AccountId));
        }

        [Test]
        public async Task SpeedTest()
        {
            var sut = new MenschIdGenerator();
            //IIdStore idStore = new InMemoryIdStore();
            IIdStore idStore = new IdStore(database);

            var maximumCount = (int)Math.Pow(MenschIdGenerator.CharacterSpace.Length, MenschIdGenerator.IdSuffixLength);
            var countOfIdsInUse = await idStore.CountIdsMatching(new Regex($"^{BirthDate:yyyyMMdd}"));
            Console.WriteLine($"IDs for requested birth date: {countOfIdsInUse} of {maximumCount}");
            if (countOfIdsInUse >= maximumCount)
            {
                Console.WriteLine("ID space is exhausted");
                throw new Exception("ID space exhausted");
            }
            var collisions = 0;
            var isIdSpaceAlmostExhausted = countOfIdsInUse > 0.8 * maximumCount;
            var lastElapsed = TimeSpan.Zero;
            var stopWatch = Stopwatch.StartNew();
            const int ReportInterval = 1_000;
            while (countOfIdsInUse < maximumCount)
            {
                var idCandidate = isIdSpaceAlmostExhausted ? await sut.FindFreeIdUsingIdStore(BirthDate, idStore) : sut.Generate(BirthDate);
                if(!await idStore.TryReserveCandidate(idCandidate, IdType.MenschID, AccountId))
                {
                    // May happen if other process has found the same ID in parallel and has already reserved it.
                    collisions++;
                    continue;
                }
                countOfIdsInUse++;
                isIdSpaceAlmostExhausted = countOfIdsInUse > 0.8 * maximumCount;
                if (countOfIdsInUse % ReportInterval == 0)
                {
                    var elapsed = stopWatch.Elapsed;
                    var diff = elapsed - lastElapsed;
                    var collisionsPerId = collisions / (double)ReportInterval;
                    Console.WriteLine($"{countOfIdsInUse};{100*collisionsPerId:F1}%;{elapsed.TotalSeconds:F3}s;{diff.TotalSeconds:F3}s");
                    lastElapsed = elapsed;
                    collisions = 0;
                }
            }
        }

        private class SuffixSearcher
        {
            private readonly List<string> existingSuffixes;

            public SuffixSearcher(
                List<string> existingSuffixes)
            {
                this.existingSuffixes = existingSuffixes;
            }

            // Note:
            // Does this make practical senese? Usually IDs are generated for different dates.
            // Loading suffixes for all of them would be enormously memory-intensive.
            //
            // TODO
            // Load all existing suffixes for a specific date
            // Use efficient data structures to count and search for available suffixes
        }

        private class InMemoryIdStore : IIdStore
        {
            private readonly ConcurrentDictionary<string, MenschId> collection = new();

            public IAsyncEnumerable<MenschId> GetAllAsync()
            {
                throw new NotImplementedException();
            }

            public async Task<bool> ExistsAsync(
                string id)
            {
                throw new NotImplementedException();
            }

            public async Task<MenschId> GetByIdAsync(
                string id)
            {
                throw new NotImplementedException();
            }

            public IAsyncEnumerable<MenschId> SearchAsync(
                Expression<Func<MenschId, bool>> filter,
                int? count = null,
                int? skip = null,
                Expression<Func<MenschId, object>> orderBy = null,
                OrderDirection orderDirection = OrderDirection.Ascending)
            {
                throw new NotImplementedException();
            }

            public async Task<MenschId> FirstOrDefaultAsync(
                Expression<Func<MenschId, bool>> filter)
            {
                throw new NotImplementedException();
            }

            public async Task<StorageOperation> StoreAsync(
                MenschId item)
            {
                throw new NotImplementedException();
            }

            public async Task DeleteAsync(
                string id)
            {
                throw new NotImplementedException();
            }

            public async Task<long> CountIdsMatching(
                Regex regex)
            {
                return collection.Keys.Count(regex.IsMatch);
            }

            public async Task<bool> TryReserveCandidate(
                string idCandidate,
                IdType idType,
                string accountId)
            {
                var menschId = new MenschId
                {
                    Id = idCandidate,
                    Type = IdType.MenschID,
                    ReservingAccountId = accountId
                };
                return collection.TryAdd(idCandidate, menschId);
            }

            public async Task<bool> TryClaimId(
                string id,
                string accountId)
            {
                throw new NotImplementedException();
            }

            public async Task UnclaimId(
                string id)
            {
                throw new NotImplementedException();
            }

            public async Task ReleaseSpecificReservation(
                string accountId,
                string idCandidate)
            {
                throw new NotImplementedException();
            }

            public async Task ReleaseReservations(
                string accountId)
            {
                throw new NotImplementedException();
            }

            public async Task ReleaseReservationsNotMatchingBirthdate(
                string accountId,
                DateTime birthDate)
            {
                throw new NotImplementedException();
            }
        }
    }
}
