﻿namespace Mensch.Id.API.Models
{
    public class Person : IId
    {
        public string Id { get; set; }
        public string AnonymousId { get; set; }
    }
}
