using Meridian.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Data;

public sealed record OrderSummary(int OrderId, string QuoteNumber, string CustomerName, DateTime CreatedOn, decimal Total);

public static class OrderQueries
{
    /// <summary>
    /// Replaces Orders.aspx.cs BindGrid(). One SQL query with a projection
    /// (no lazy loading, no N+1). Region filtering preserved exactly: non-admins
    /// see only their region. Both EF6 (UseDatabaseNullSemantics = false, the
    /// default) and EF Core translate == with C# null semantics, so a user whose
    /// region is null sees orders for customers whose region is also null.
    /// That is odd, but it is the legacy behaviour; changing it is a separate,
    /// approved decision (see docs/decisions.md, D-007).
    /// </summary>
    public static IQueryable<OrderSummary> RecentOrders(
        this MeridianDbContext db, string? status, bool isAdmin, string? userRegion, int take = 500)
    {
        IQueryable<Order> query = db.Orders.AsNoTracking();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(o => o.Status == status);
        }

        if (!isAdmin)
        {
            query = query.Where(o => o.Customer!.Region == userRegion);
        }

        return query
            .OrderByDescending(o => o.CreatedOn)
            .Take(take)
            .Select(o => new OrderSummary(o.OrderId, o.QuoteNumber, o.Customer!.Name, o.CreatedOn, o.Total));
    }
}
