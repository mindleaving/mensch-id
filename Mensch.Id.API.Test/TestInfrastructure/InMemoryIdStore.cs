using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mensch.Id.API.Models;
using Mensch.Id.API.Storage;

namespace Mensch.Id.API.Test.TestInfrastructure;

public class InMemoryIdStore : InMemoryStore<MenschId>, IIdStore
{
    public Task<bool> TryReserveCandidate(
        string idCandidate,
        IdType idType,
        string accountId)
    {
        var menschId = new MenschId
        {
            Id = idCandidate,
            Type = idType,
            ReservingAccountId = accountId
        };
        var isReserved = items.TryAdd(idCandidate, menschId);
        return Task.FromResult(isReserved);
    }

    public Task<bool> TryClaimId(
        string id,
        string accountId)
    {
        if (!items.TryGetValue(id, out var menschId) || menschId.ReservingAccountId != accountId)
            return Task.FromResult(false);
        menschId.IsClaimed = true;
        return Task.FromResult(true);
    }

    public Task UnclaimId(string id)
    {
        if (items.TryGetValue(id, out var menschId))
            menschId.IsClaimed = false;
        return Task.CompletedTask;
    }

    public Task ReleaseSpecificReservation(
        string accountId,
        string idCandidate)
    {
        var isReleasable = items.TryGetValue(idCandidate, out var menschId)
                           && !menschId.IsClaimed
                           && menschId.ReservingAccountId == accountId;
        if (isReleasable)
            items.TryRemove(idCandidate, out _);
        return Task.CompletedTask;
    }

    public Task ReleaseReservations(string accountId)
    {
        var releasedIds = items.Values
            .Where(x => !x.IsClaimed && x.ReservingAccountId == accountId)
            .Select(x => x.Id)
            .ToList();
        RemoveAll(releasedIds);
        return Task.CompletedTask;
    }

    public Task ReleaseReservationsNotMatchingBirthdate(
        string accountId,
        DateOnly birthDate)
    {
        var prefix = birthDate.ToString("yyyyMMdd");
        var releasedIds = items.Values
            .Where(x => !x.IsClaimed && x.ReservingAccountId == accountId && x.Type == IdType.MenschID && !x.Id.StartsWith(prefix))
            .Select(x => x.Id)
            .ToList();
        RemoveAll(releasedIds);
        return Task.CompletedTask;
    }

    private void RemoveAll(List<string> ids)
    {
        foreach (var id in ids)
        {
            items.TryRemove(id, out _);
        }
    }
}
