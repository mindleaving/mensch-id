using System;
using System.Threading.Tasks;
using Mensch.Id.API.Models;

namespace Mensch.Id.API.Storage;

public class OneTimeFileMapStore
{
    private readonly IStore<OneTimeFileMap> backingStore;

    public OneTimeFileMapStore(
        IStore<OneTimeFileMap> backingStore)
    {
        this.backingStore = backingStore;
    }

    public async Task<string> GetFileId(
        string oneTimeId)
    {
        var oneTimeMap = await backingStore.GetByIdAsync(oneTimeId);
        if (oneTimeMap == null)
            return null;
        await backingStore.DeleteAsync(oneTimeId);
        return oneTimeMap.FileId;
    }

    public async Task<string> CreateNew(
        string fileId)
    {
        var oneTimeId = Guid.NewGuid().ToString();
        var oneTimeMap = new OneTimeFileMap(oneTimeId, fileId);
        await backingStore.StoreAsync(oneTimeMap);
        return oneTimeMap.Id;
    }
}