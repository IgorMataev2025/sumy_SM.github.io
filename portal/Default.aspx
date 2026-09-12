<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Default.aspx.vb" Inherits="SumyPortal.Default" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Головна — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <section class="hero">
        <h1><asp:Literal runat="server" Text="<%$ Resources:SiteText, Home_HeroTitle %>" /></h1>
        <p>
            <asp:Literal runat="server" Text="<%$ Resources:SiteText, Home_HeroText %>" />
        </p>
    </section>

    <section class="categories">
        <h2><asp:Literal runat="server" Text="<%$ Resources:SiteText, Home_CategoriesHeading %>" /></h2>
        <asp:Panel ID="dbErrorPanel" runat="server" Visible="false" CssClass="stub-note">
            <asp:Literal runat="server" Text="<%$ Resources:SiteText, Home_DbError %>" />
        </asp:Panel>
        <asp:Repeater ID="rptCategories" runat="server">
            <ItemTemplate>
                <a class="category-card" href='<%#: "Catalog.aspx?categoryId=" & CType(Container.DataItem, SumyPortal.ServiceCategory).CategoryId %>'>
                    <h3><%#: CType(Container.DataItem, SumyPortal.ServiceCategory).Name %></h3>
                    <p><%#: CType(Container.DataItem, SumyPortal.ServiceCategory).Description %></p>
                </a>
            </ItemTemplate>
        </asp:Repeater>
    </section>
</asp:Content>
