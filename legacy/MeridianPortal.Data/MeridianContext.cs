using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace MeridianPortal.Data
{
    public class MeridianContext : DbContext
    {
        public MeridianContext() : base("name=MeridianDb")
        {
            // Lazy loading is ON by default in EF6; pages rely on it implicitly.
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Configurations.Add(new CustomerMap());
            modelBuilder.Configurations.Add(new OrderMap());
            modelBuilder.Entity<Product>().ToTable("tblProduct");
            modelBuilder.Entity<OrderLine>().ToTable("tblOrderLine");
        }
    }

    public class CustomerMap : EntityTypeConfiguration<Customer>
    {
        public CustomerMap()
        {
            ToTable("tblCustomer");
            Property(c => c.AccountCode).HasMaxLength(12).IsRequired();
            Property(c => c.Name).HasMaxLength(200).IsRequired();
        }
    }

    public class OrderMap : EntityTypeConfiguration<Order>
    {
        public OrderMap()
        {
            ToTable("tblOrder");
            Property(o => o.QuoteNumber).HasMaxLength(20).IsRequired();
            Property(o => o.Status).HasMaxLength(1).IsFixedLength();
            Property(o => o.RowVersion).IsRowVersion();
            HasRequired(o => o.Customer).WithMany(c => c.Orders).HasForeignKey(o => o.CustomerId);
        }
    }
}
