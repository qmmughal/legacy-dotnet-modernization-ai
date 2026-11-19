using Meridian.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Data.Tests;

/// <summary>
/// The EF Core model must map onto the EXISTING legacy schema. These tests use
/// the SQL Server provider's model (no database needed) because production
/// runs on SQL Server; SQLite is only the test database.
/// </summary>
public sealed class MappingTests
{
    private static MeridianDbContext SqlServerModelContext() => new(
        new DbContextOptionsBuilder<MeridianDbContext>()
            .UseSqlServer("Server=unused;Database=unused;TrustServerCertificate=True")
            .Options);

    [Theory]
    [InlineData(typeof(Customer), "tblCustomer")]
    [InlineData(typeof(Product), "tblProduct")]
    [InlineData(typeof(Order), "tblOrder")]
    [InlineData(typeof(OrderLine), "tblOrderLine")]
    public void Entities_MapToLegacyTables(Type entity, string table)
    {
        using var db = SqlServerModelContext();
        Assert.Equal(table, db.Model.FindEntityType(entity)!.GetTableName());
    }

    [Fact]
    public void Money_IsDecimal18_2_LikeEf6()
    {
        using var db = SqlServerModelContext();
        var total = db.Model.FindEntityType(typeof(Order))!.FindProperty(nameof(Order.Total))!;
        Assert.Equal("decimal(18,2)", total.GetColumnType());
    }

    [Fact]
    public void OrderStatus_IsFixedLengthChar1()
    {
        using var db = SqlServerModelContext();
        var status = db.Model.FindEntityType(typeof(Order))!.FindProperty(nameof(Order.Status))!;
        Assert.Equal("nchar(1)", status.GetColumnType());
    }

    [Fact]
    public void RowVersion_IsConcurrencyToken()
    {
        using var db = SqlServerModelContext();
        var rv = db.Model.FindEntityType(typeof(Order))!.FindProperty(nameof(Order.RowVersion))!;
        Assert.True(rv.IsConcurrencyToken);
        Assert.Equal("rowversion", rv.GetColumnType());
    }
}
