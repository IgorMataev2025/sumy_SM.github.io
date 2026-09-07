<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="ServiceDetails.aspx.vb" Inherits="SumyPortal.ServiceDetails" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    <asp:Literal ID="titleLiteral" runat="server" /> — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <p><a href="Catalog.aspx">← До каталогу</a></p>

    <asp:Panel ID="notFoundPanel" runat="server" Visible="false" CssClass="stub-note">
        Оголошення не знайдено — можливо, його зняли з публікації.
    </asp:Panel>

    <asp:Panel ID="detailsPanel" runat="server" CssClass="service-details">
        <h1><asp:Literal ID="headingLiteral" runat="server" /></h1>
        <p class="service-category"><asp:Literal ID="categoryLiteral" runat="server" /></p>

        <div class="catalog-grid photo-gallery">
            <asp:Repeater ID="rptPhotos" runat="server">
                <ItemTemplate>
                    <img class="gallery-photo" src='<%#: ResolveUrl(CType(Container.DataItem, SumyPortal.ServicePhoto).FilePath) %>' alt="" />
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <p class="catalog-price"><asp:Literal ID="priceLiteral" runat="server" /></p>

        <p><asp:Literal ID="descriptionLiteral" runat="server" /></p>

        <div class="contact-box">
            <h2>Контакти постачальника</h2>
            <p><asp:Literal ID="providerNameLiteral" runat="server" /></p>
            <p>Телефон: <asp:Literal ID="phoneLiteral" runat="server" /></p>
            <p>Район: <asp:Literal ID="districtLiteral" runat="server" /></p>
            <p><asp:HyperLink ID="contractLink" runat="server" CssClass="btn-primary" Visible="false">Сформувати договір</asp:HyperLink></p>
        </div>
    </asp:Panel>
</asp:Content>
