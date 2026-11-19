using Meridian.Domain.Orders;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Data;

/// <summary>
/// EF Core replacement for the EF6 MeridianContext. Maps to the SAME legacy
/// tables (tblCustomer, tblOrder, ...) so both applications can share the
/// database during the side-by-side period. Identity tables are new.
/// </summary>
public sealed class MeridianDbContext(DbContextOptions<MeridianDbContext> options)
    : IdentityDbContext<PortalUser>(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(MeridianDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // EF6 mapped decimal to decimal(18,2) by convention; EF Core warns and
        // picks provider defaults unless told. Pin it to match the legacy schema.
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
    }
}
