using Meridian.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meridian.Data;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> b)
    {
        b.ToTable("tblCustomer");
        b.HasKey(c => c.CustomerId);
        b.Property(c => c.AccountCode).HasMaxLength(12);
        b.Property(c => c.Name).HasMaxLength(200);
        b.HasIndex(c => c.AccountCode).IsUnique();
    }
}

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        b.ToTable("tblProduct");
        b.HasKey(p => p.ProductId);
        // EF6 mapped unconstrained strings to nvarchar(max). Keep that unless the
        // schema is changed deliberately in its own migration. No index here:
        // SQL Server cannot index an nvarchar(max) column (SQLite would not complain).
        b.Property(p => p.Code);
    }
}

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.ToTable("tblOrder");
        b.HasKey(o => o.OrderId);
        b.Property(o => o.QuoteNumber).HasMaxLength(20);
        b.Property(o => o.Status).HasMaxLength(1).IsFixedLength();
        b.Property(o => o.RowVersion).IsRowVersion();
        b.HasOne(o => o.Customer).WithMany(c => c.Orders).HasForeignKey(o => o.CustomerId);
        b.HasMany(o => o.Lines).WithOne(l => l.Order).HasForeignKey(l => l.OrderId);
    }
}

internal sealed class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> b)
    {
        b.ToTable("tblOrderLine");
        b.HasKey(l => l.OrderLineId);
        b.HasOne(l => l.Product).WithMany().HasForeignKey(l => l.ProductId);
    }
}
