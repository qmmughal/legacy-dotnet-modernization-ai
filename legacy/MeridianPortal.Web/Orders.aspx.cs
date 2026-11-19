using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using MeridianPortal.Data;

namespace MeridianPortal.Web
{
    public partial class Orders : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!User.Identity.IsAuthenticated)
                Response.Redirect("~/Account/Login.aspx?ReturnUrl=" + Server.UrlEncode(Request.RawUrl));

            if (!IsPostBack)
            {
                StatusFilter.SelectedValue = (string)Session["OrdersStatusFilter"] ?? "";
                BindGrid();
            }
        }

        protected void StatusFilter_Changed(object sender, EventArgs e)
        {
            Session["OrdersStatusFilter"] = StatusFilter.SelectedValue;
            OrdersGrid.PageIndex = 0;
            BindGrid();
        }

        protected void OrdersGrid_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            OrdersGrid.PageIndex = e.NewPageIndex;
            BindGrid();
        }

        private void BindGrid()
        {
            using (var db = new MeridianContext())
            {
                var query = db.Orders.AsQueryable();
                var status = StatusFilter.SelectedValue;
                if (!string.IsNullOrEmpty(status))
                    query = query.Where(o => o.Status == status);

                // Sales reps see only their region; admins see everything.
                if (!User.IsInRole("Admin"))
                {
                    var region = (string)Session["UserRegion"];
                    query = query.Where(o => o.Customer.Region == region);
                }

                OrdersGrid.DataSource = query
                    .OrderByDescending(o => o.CreatedOn)
                    .Take(500)
                    .ToList()   // materialize, then lazy-load Customer per row (N+1)
                    .Select(o => new { o.OrderId, o.QuoteNumber, CustomerName = o.Customer.Name, o.CreatedOn, o.Total })
                    .ToList();
                OrdersGrid.DataBind();
            }
        }
    }
}
