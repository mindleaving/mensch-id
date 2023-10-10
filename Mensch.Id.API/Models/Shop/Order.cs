using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Commons.Extensions;

namespace Mensch.Id.API.Models.Shop
{
    public class Order : IId
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string OrderedByAccountId { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }
        public bool IsPaymentReceived { get; set; }

        [Required]
        public Contact InvoiceAddress { get; set; }

        public bool SendInvoiceSeparately { get; set; }

        [Required]
        public ShippingMethod ShippingMethod { get; set; }
        [Required]
        public Contact ShippingAddress { get; set; }

        [Required]
        public List<OrderItem> Items { get; set; }

        private List<OrderStatusChange> statusChanges = new();
        public List<OrderStatusChange> StatusChanges
        {
            get => statusChanges;
            private set => statusChanges = value ?? new();
        }
        public OrderStatus Status { get; private set; }

        public void SetStatus(
            OrderStatus newStatus)
        {
            if (StatusChanges.Count == 0)
            {
                if (newStatus != OrderStatus.Placed)
                    throw new Exception("First status of an order must be 'Placed'");
            } 
            else
            {
                var currentStatus = StatusChanges.Last().NewStatus;
                if(currentStatus == newStatus)
                    return;
                if (currentStatus.InSet(OrderStatus.Cancelled, OrderStatus.Returned))
                    throw new InvalidOperationException("Order is in a final state and cannot be changed");
                if (newStatus == OrderStatus.Cancelled && currentStatus != OrderStatus.Placed)
                    throw new InvalidOperationException("Cannot cancel order that is not in 'Placed' status");
            }

            Status = newStatus;
            StatusChanges.Add(new OrderStatusChange(newStatus, DateTime.UtcNow));
        }
    }

    public class OrderStatusChange
    {
        public OrderStatusChange() { }
        public OrderStatusChange(
            OrderStatus newStatus,
            DateTime timestamp)
        {
            NewStatus = newStatus;
            Timestamp = timestamp;
        }

        public OrderStatus NewStatus { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
