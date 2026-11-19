namespace Meridian.Domain.Orders;

public sealed class Customer
{
    public int CustomerId { get; set; }
    public required string AccountCode { get; set; }
    public required string Name { get; set; }
    public string? Tier { get; set; }
    public string? Region { get; set; }
    public List<Order> Orders { get; } = [];
}

public sealed class Product
{
    public int ProductId { get; set; }
    public required string Code { get; set; }
    public string? Description { get; set; }
    public decimal ListPrice { get; set; }
    public double WeightKg { get; set; }
    public bool Discontinued { get; set; }
}

public static class OrderStatus
{
    public const string Quote = "Q";
    public const string Ordered = "O";
    public const string Cancelled = "X";
}

public sealed class Order
{
    public int OrderId { get; set; }
    public required string QuoteNumber { get; set; }
    public int CustomerId { get; set; }

    /// <summary>Server local time (legacy convention). Kept as DateTime, Kind=Unspecified.</summary>
    public DateTime CreatedOn { get; set; }

    public string Status { get; set; } = OrderStatus.Quote;
    public decimal Total { get; set; }
    public byte[]? RowVersion { get; set; }
    public Customer? Customer { get; set; }
    public List<OrderLine> Lines { get; } = [];
}

public sealed class OrderLine
{
    public int OrderLineId { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public Order? Order { get; set; }
    public Product? Product { get; set; }
}
