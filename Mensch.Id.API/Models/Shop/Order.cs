using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Mensch.Id.API.Models.Shop
{
    public class Order : IId
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public DateTime CreationTimestamp { get; set; }

        [Required]
        public string OrderedByAccountId { get; set; }

        [Required]
        public Address ShippingAddress { get; set; }

        [Required]
        public List<OrderItem> Items { get; set; }

        public OrderStatus Status { get; set; }
        public DateTime? FulfilledTimestamp { get; set; }
    }

    public enum OrderStatus
    {
        Placed,
        Fulfilled,
        Cancelled
    }
}
