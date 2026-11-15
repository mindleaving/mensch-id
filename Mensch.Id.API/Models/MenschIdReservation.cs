using Newtonsoft.Json;
using TypescriptGenerator.Attributes;

namespace Mensch.Id.API.Models;

public class MenschIdReservation : IId
{
    public string Id { get; set; }
    public IdType Type { get; set; }
    public bool IsClaimed { get; set; }
    [JsonIgnore]
    [TypescriptIsOptional]
    public string ReservingAccountId { get; set; }
}
