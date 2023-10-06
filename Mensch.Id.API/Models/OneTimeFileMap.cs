namespace Mensch.Id.API.Models;

public class OneTimeFileMap : IId
{
    public OneTimeFileMap(
        string id,
        string fileId)
    {
        Id = id;
        FileId = fileId;
    }

    public string Id { get; set; }
    public string FileId { get; set; }
}