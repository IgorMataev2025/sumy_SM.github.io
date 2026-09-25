<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Favorites.aspx.vb" Inherits="SumyPortal.Favorites" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    <asp:Literal runat="server" Text="<%$ Resources:SiteText, Nav_Favorites %>" /> — <asp:Literal runat="server" Text="<%$ Resources:SiteText, SiteTitleSuffix %>" />
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Literal runat="server" Text="<%$ Resources:SiteText, Nav_Favorites %>" /></h1>

    <asp:Panel ID="emptyPanel" runat="server" Visible="false" CssClass="stub-note">
        <asp:Literal runat="server" Text="<%$ Resources:SiteText, Favorites_Empty_Prefix %>" />
        <a href="Catalog.aspx"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Favorites_Empty_CatalogLink %>" /></a>
        <asp:Literal runat="server" Text="<%$ Resources:SiteText, Favorites_Empty_Suffix %>" />
    </asp:Panel>

    <div class="catalog-grid">
        <asp:Repeater ID="rptFavorites" runat="server" OnItemCommand="rptFavorites_ItemCommand">
            <ItemTemplate>
                <%-- 2026-09-25: фото як у каталозі, "Прибрати" прямо тут (кнопка під карткою —
                     всередині <a> її не можна), зняте з публікації — без посилання (ServiceDetails
                     його однаково не покаже), приглушене, з позначкою "Недоступне". --%>
                <div>
                <asp:HyperLink runat="server" CssClass="catalog-card"
                    NavigateUrl='<%#: If(IsAvailable(Container.DataItem), "~/ServiceDetails.aspx?id=" & CType(Container.DataItem, SumyPortal.Service).ServiceId, "") %>'
                    style='<%#: If(IsAvailable(Container.DataItem), "", "opacity:0.55;") %>'>
                    <div class="catalog-thumb">
                        <asp:Image runat="server" Visible='<%#: Not String.IsNullOrEmpty(CType(Container.DataItem, SumyPortal.Service).ThumbnailUrl) %>'
                            ImageUrl='<%#: If(String.IsNullOrEmpty(CType(Container.DataItem, SumyPortal.Service).ThumbnailUrl), "", ResolveUrl(CType(Container.DataItem, SumyPortal.Service).ThumbnailUrl)) %>' AlternateText='<%#: CType(Container.DataItem, SumyPortal.Service).Title %>' />
                        <span class="catalog-thumb-placeholder" runat="server" visible='<%#: String.IsNullOrEmpty(CType(Container.DataItem, SumyPortal.Service).ThumbnailUrl) %>'><asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_Photo_Placeholder %>" /></span>
                    </div>
                    <div class="catalog-card-body">
                        <h3><%#: CType(Container.DataItem, SumyPortal.Service).Title %></h3>
                        <asp:Label runat="server" CssClass="status-badge" Visible='<%# Not IsAvailable(Container.DataItem) %>'
                            Text="<%$ Resources:SiteText, Favorites_Unavailable %>" />
                        <p class="service-category">
                            <%#: CType(Container.DataItem, SumyPortal.Service).CategoryName %>
                            <%#: If(String.IsNullOrEmpty(CType(Container.DataItem, SumyPortal.Service).District), "", " · " & CType(Container.DataItem, SumyPortal.Service).District) %>
                        </p>
                        <p class="catalog-price">
                            <%#: If(CType(Container.DataItem, SumyPortal.Service).Price.HasValue, CType(Container.DataItem, SumyPortal.Service).Price.Value.ToString("0.## грн"), Resources.SiteText.Price_Negotiable) %>
                        </p>
                    </div>
                </asp:HyperLink>
                <asp:Button runat="server" CommandName="Remove" CausesValidation="false" CssClass="btn-secondary"
                    style="width:100%;margin-top:0.4rem;padding:0.3rem 0.8rem;font-size:0.9rem;"
                    CommandArgument='<%#: CType(Container.DataItem, SumyPortal.Service).ServiceId %>'
                    Text="<%$ Resources:SiteText, Details_Favorite_Remove %>" />
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>
</asp:Content>
