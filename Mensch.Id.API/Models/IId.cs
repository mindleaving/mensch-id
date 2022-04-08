using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace Mensch.Id.API.Models
{
    public interface IId
    {
        [BsonId]
        [Required]
        string Id { get; }
    }
}