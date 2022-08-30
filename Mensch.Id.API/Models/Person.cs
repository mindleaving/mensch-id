using System;

namespace Mensch.Id.API.Models
{
    public class Person : IId
    {
        public Person(string id, DateTime creationDate)
        {
            Id = id;
            CreationDate = creationDate;
        }

        public string Id { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
