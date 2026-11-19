using System;
using System.Collections.Generic;

namespace MeridianPortal.Data
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string AccountCode { get; set; }
        public string Name { get; set; }
        public string Tier { get; set; }      // "GOLD", "SILVER", or null
        public string Region { get; set; }    // "NORTH", "SOUTH", "EXPORT"
        public virtual ICollection<Order> Orders { get; set; }
    }

    public class Product
    {
        public int ProductId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public decimal ListPrice { get; set; }
        public double WeightKg { get; set; }
        public bool Discontinued { get; set; }
    }

    public class Order
    {
        public int OrderId { get; set; }
        public string QuoteNumber { get; set; }
        public int CustomerId { get; set; }
        public DateTime CreatedOn { get; set; }   // server local time, never UTC
        public string Status { get; set; }       // "Q" quote, "O" ordered, "X" cancelled
        public decimal Total { get; set; }
        public byte[] RowVersion { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual ICollection<OrderLine> Lines { get; set; }
    }

    public class OrderLine
    {
        public int OrderLineId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }
}
