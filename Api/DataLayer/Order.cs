using System;
using System.Collections.Generic;

namespace Api
{
    public class Order
    {
        public Guid Id { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime Date { get; set; }

        public ICollection<OrderLine> Lines { get; set; } = new List<OrderLine>();

        public Guid UserId { get; set; }
    }

    public class OrderLine
    {
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public string ProductNumber { get; set; }

        public decimal Quantity { get; set; }

        public decimal Amount { get; set; }

        public decimal Vat { get; set; }
    }
}