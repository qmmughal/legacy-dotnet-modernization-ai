using Meridian.Domain.Orders;

namespace Meridian.Data;

/// <summary>Small, fictional data set for development and tests.</summary>
public static class SeedData
{
    public static void Populate(MeridianDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        if (db.Customers.Any())
        {
            return;
        }

        var north = new Customer { AccountCode = "C-1001", Name = "Northwind Fabrication", Tier = "GOLD", Region = "NORTH" };
        var south = new Customer { AccountCode = "C-2002", Name = "Southbank Pumps", Tier = "SILVER", Region = "SOUTH" };
        var export = new Customer { AccountCode = "C-3003", Name = "Harbor Export Ltd", Tier = null, Region = "EXPORT" };
        db.Customers.AddRange(north, south, export);

        var bolt = new Product { Code = "BLT-M8-40", Description = "Hex bolt M8x40, zinc", ListPrice = 0.42m, WeightKg = 0.021 };
        var valve = new Product { Code = "VLV-200", Description = "Gate valve DN50", ListPrice = 129.90m, WeightKg = 6.4 };
        var solvent = new Product { Code = "HAZ-SOLV-5", Description = "Degreasing solvent 5 L", ListPrice = 38.15m, WeightKg = 5.2 };
        db.Products.AddRange(bolt, valve, solvent);

        var created = new DateTime(2026, 11, 2, 9, 30, 0, DateTimeKind.Unspecified);
        db.Orders.AddRange(
            new Order { QuoteNumber = "Q-20261102-00001", Customer = north, CreatedOn = created, Status = OrderStatus.Ordered, Total = 1243.56m },
            new Order { QuoteNumber = "Q-20261103-00002", Customer = south, CreatedOn = created.AddDays(1), Status = OrderStatus.Quote, Total = 172.04m },
            new Order { QuoteNumber = "Q-20261104-00003", Customer = export, CreatedOn = created.AddDays(2), Status = OrderStatus.Quote, Total = 64.17m });

        db.SaveChanges();
    }
}
