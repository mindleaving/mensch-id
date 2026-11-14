using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace Mensch.Id.API.Models;

public interface IId<out TKey>
{
    [BsonId]
    [Required]
    TKey Id { get; }
}

public interface IId : IId<string>
{
}
