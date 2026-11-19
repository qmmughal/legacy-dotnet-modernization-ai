<%@ Page Title="Orders" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Orders.aspx.cs" Inherits="MeridianPortal.Web.Orders" %>
<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <h2>Recent orders</h2>
    <asp:DropDownList ID="StatusFilter" runat="server" AutoPostBack="true" OnSelectedIndexChanged="StatusFilter_Changed">
        <asp:ListItem Value="" Text="All" />
        <asp:ListItem Value="Q" Text="Quotes" />
        <asp:ListItem Value="O" Text="Orders" />
    </asp:DropDownList>
    <asp:GridView ID="OrdersGrid" runat="server" AutoGenerateColumns="false" AllowPaging="true" PageSize="20"
                  OnPageIndexChanging="OrdersGrid_PageIndexChanging" DataKeyNames="OrderId">
        <Columns>
            <asp:HyperLinkField DataTextField="QuoteNumber" DataNavigateUrlFields="OrderId"
                                DataNavigateUrlFormatString="~/OrderDetail.aspx?id={0}" HeaderText="Quote" />
            <asp:BoundField DataField="CustomerName" HeaderText="Customer" />
            <asp:BoundField DataField="CreatedOn" HeaderText="Created" DataFormatString="{0:d}" />
            <asp:BoundField DataField="Total" HeaderText="Total" DataFormatString="{0:C}" />
        </Columns>
    </asp:GridView>
</asp:Content>
