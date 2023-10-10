using System;

namespace Mensch.Id.API.Models
{
    public class Person : IId
    {
        public Person(
            string id, 
            DateTime creationDate,
            string creationAccountId)
        {
            Id = id;
            CreationDate = creationDate;
            CreationAccountId = creationAccountId;
        }

        public string Id { get; set; }
        public DateTime CreationDate { get; set; }
        public string CreationAccountId { get; set; }
    }
}
