using System;
using System.Collections.Generic;

namespace Api
{
    public class Order
    {
        public Guid Id { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime Date { get; set; }

        public ICollection<Product> Products { get; set; }

        public Guid UserId { get; set; }
    }

    public class Product
    {
        public string Number { get; set; }

        public decimal Quantity { get; set; }

        public decimal Amount { get; set; }

        public decimal Vat { get; set; }
    }
}