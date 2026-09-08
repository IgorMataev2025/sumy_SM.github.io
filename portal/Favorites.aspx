<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Favorites.aspx.vb" Inherits="SumyPortal.Favorites" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Обране — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Обране</h1>

    <asp:Panel ID="emptyPanel" runat="server" Visible="false" CssClass="stub-note">
        Ви ще не додали жодного оголошення в обране — знайдіть цікаве в
        <a href="Catalog.aspx">каталозі</a> і натисніть «Додати в обране» на його сторінці.
    </asp:Panel>

    <div class="catalog-grid">
        <asp:Repeater ID="rptFavorites" runat="server">
            <ItemTemplate>
                <a class="catalog-card" href='<%#: "ServiceDetails.aspx?id=" & CType(Container.DataItem, SumyPortal.Service).ServiceId %>'>
                    <div class="catalog-card-body">
                        <h3><%#: CType(Container.DataItem, SumyPortal.Service).Title %></h3>
                        <p class="service-category">
                            <%#: CType(Container.DataItem, SumyPortal.Service).CategoryName %>
                            <%#: If(String.IsNullOrEmpty(CType(Container.DataItem, SumyPortal.Service).District), "", " · " & CType(Container.DataItem, SumyPortal.Service).District) %>
                        </p>
                        <p class="catalog-price">
                            <%#: If(CType(Container.DataItem, SumyPortal.Service).Price.HasValue, CType(Container.DataItem, SumyPortal.Service).Price.Value.ToString("0.## грн"), "Ціна за домовленістю") %>
                        </p>
                    </div>
                </a>
            </ItemTemplate>
        </asp:Repeater>
    </div>
</asp:Content>
