<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="ServiceDetails.aspx.vb" Inherits="SumyPortal.ServiceDetails" %>
<%@ MasterType VirtualPath="~/Site.master" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    <asp:Literal ID="titleLiteral" runat="server" /> — Портал послуг Safina
</asp:Content>
<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Leaflet + OpenStreetMap — той самий CDN, що ServiceEdit.aspx (постановка робочої тестової версії, 2026-09-12). -->
    <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"
        integrity="sha256-p4NxAoJBhIIN+hmNHrzRCf9tD/miZyoHS5obTRR9BMY=" crossorigin="" />
    <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"
        integrity="sha256-20nQCchB9co0qIjJZRGuk2/Z9VM+kNiyxNV1lvTlZBo=" crossorigin=""></script>
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <p><a href="Catalog.aspx"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Details_BackToCatalog %>" /></a></p>

    <asp:Panel ID="notFoundPanel" runat="server" Visible="false" CssClass="stub-note">
        <asp:Literal runat="server" Text="<%$ Resources:SiteText, Details_NotFound %>" />
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
            <h2><asp:Literal runat="server" Text="<%$ Resources:SiteText, Details_ContactsHeading %>" /></h2>
            <p><asp:Literal ID="providerNameLiteral" runat="server" /></p>
            <p><asp:HyperLink ID="providerGalleryLink" runat="server" CssClass="btn-secondary" Text="<%$ Resources:SiteText, Details_GalleryLink %>" /></p>
            <asp:PlaceHolder ID="phoneHolder" runat="server">
                <p><asp:Literal runat="server" Text="<%$ Resources:SiteText, Details_PhoneLabel %>" /> <asp:Literal ID="phoneLiteral" runat="server" /></p>
            </asp:PlaceHolder>
            <asp:Panel ID="anonContactPanel" runat="server" Visible="false" CssClass="stub-note">
                <asp:Literal runat="server" Text="<%$ Resources:SiteText, Details_AnonContact_Prefix %>" /><a href="Login.aspx"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Details_AnonContact_Login %>" /></a><asp:Literal runat="server" Text="<%$ Resources:SiteText, Details_AnonContact_Or %>" /><a href="Register.aspx"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Details_AnonContact_Register %>" /></a>.
            </asp:Panel>
            <p><asp:Literal runat="server" Text="<%$ Resources:SiteText, Details_DistrictLabel %>" /> <asp:Literal ID="districtLiteral" runat="server" /></p>
            <asp:Panel ID="mapPanel" runat="server" Visible="false">
                <p><b><asp:Literal runat="server" Text="<%$ Resources:SiteText, Details_MapHeading %>" /></b></p>
                <div id="detailsMap" style="height:250px; border-radius:8px;"></div>
                <!-- Скрипт усередині mapPanel навмисно — Visible="false" (немає мітки) не рендерить
                     дітей узагалі, тож detailsMap у DOM не буде, а координати нема чим підставити. -->
                <script>
                    (function () {
                        var map = L.map('detailsMap').setView([<%= LatitudeForScript %>, <%= LongitudeForScript %>], 15);
                        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                            attribution: '&copy; OpenStreetMap contributors',
                            maxZoom: 19
                        }).addTo(map);
                        L.marker([<%= LatitudeForScript %>, <%= LongitudeForScript %>]).addTo(map);
                    })();
                </script>
            </asp:Panel>
            <p><asp:HyperLink ID="contractLink" runat="server" CssClass="btn-primary" Visible="false" Text="<%$ Resources:SiteText, Details_ContractLink %>" /></p>
            <p>
                <asp:Button ID="btnToggleFavorite" runat="server" Visible="false" CausesValidation="false"
                    OnClick="btnToggleFavorite_Click" CssClass="btn-secondary" />
                <asp:HyperLink ID="messageLink" runat="server" CssClass="btn-secondary" Visible="false" Text="<%$ Resources:SiteText, Details_MessageLink %>" />
            </p>
        </div>

        <div class="reviews-section">
            <h2><asp:Literal runat="server" Text="<%$ Resources:SiteText, Details_ReviewsHeading %>" /></h2>
            <p class="rating-summary"><asp:Literal ID="ratingSummaryLiteral" runat="server" /></p>

            <asp:Repeater ID="rptReviews" runat="server" OnItemDataBound="rptReviews_ItemDataBound" OnItemCommand="rptReviews_ItemCommand">
                <ItemTemplate>
                    <div class="review-card">
                        <p class="review-meta">
                            <b><%#: CType(Container.DataItem, SumyPortal.Review).ConsumerName %></b> ·
                            <%#: New String("★"c, CType(Container.DataItem, SumyPortal.Review).Rating) & New String("☆"c, 5 - CType(Container.DataItem, SumyPortal.Review).Rating) %> ·
                            <%#: CType(Container.DataItem, SumyPortal.Review).CreatedAt.ToString("dd.MM.yyyy") %>
                        </p>
                        <asp:Literal runat="server" Visible='<%#: Not String.IsNullOrEmpty(CType(Container.DataItem, SumyPortal.Review).Comment) %>'
                            Text='<%#: CType(Container.DataItem, SumyPortal.Review).Comment %>' />

                        <!-- Відповідь постачальника (п.19, наступна фіча понад MVP, 2026-09-12) —
                             готова відповідь бачить будь-хто (Visible виставляється в
                             rptReviews_ItemDataBound, коли ProviderReply не порожній). -->
                        <asp:Panel ID="providerReplyPanel" runat="server" Visible="false" CssClass="stub-note">
                            <b><asp:Literal runat="server" Text="<%$ Resources:SiteText, Details_ProviderReplyHeading %>" /></b>
                            <asp:Literal ID="providerReplyLiteral" runat="server" />
                        </asp:Panel>

                        <!-- Форма відповіді — лише власнику оголошення (Visible виставляється в
                             rptReviews_ItemDataBound); той самий контрол і для першої відповіді,
                             і для редагування вже написаної (просто перезаписує, без версій). -->
                        <asp:Panel ID="ownerReplyFormPanel" runat="server" Visible="false" CssClass="form-row">
                            <label>
                                <asp:Literal runat="server" Text="<%$ Resources:SiteText, Details_ProviderReplyLabel %>" />
                            </label>
                            <asp:TextBox ID="txtProviderReply" runat="server" TextMode="MultiLine" Rows="2" MaxLength="1000"
                                Text='<%#: CType(Container.DataItem, SumyPortal.Review).ProviderReply %>' />
                            <asp:LinkButton runat="server" CommandName="Reply" CausesValidation="false"
                                CommandArgument='<%#: CType(Container.DataItem, SumyPortal.Review).ReviewId %>'
                                Text="<%$ Resources:SiteText, Details_ProviderReplyBtn %>" CssClass="btn-secondary" />
                        </asp:Panel>
                    </div>
                </ItemTemplate>
            </asp:Repeater>

            <asp:Panel ID="noReviewsPanel" runat="server" Visible="false" CssClass="stub-note">
                <asp:Literal runat="server" Text="<%$ Resources:SiteText, Details_NoReviewsPanel %>" />
            </asp:Panel>

            <asp:Panel ID="alreadyReviewedPanel" runat="server" Visible="false" CssClass="stub-note">
                <asp:Literal runat="server" Text="<%$ Resources:SiteText, Details_AlreadyReviewed %>" />
            </asp:Panel>

            <asp:Panel ID="reviewFormPanel" runat="server" Visible="false" CssClass="auth-form">
                <h3><asp:Literal runat="server" Text="<%$ Resources:SiteText, Details_LeaveReviewHeading %>" /></h3>
                <asp:ValidationSummary ID="reviewValidationSummary" runat="server" CssClass="form-error" DisplayMode="BulletList" />
                <asp:Label ID="reviewErrorLabel" runat="server" CssClass="form-error" Visible="false" />
                <div class="form-row">
                    <label for="<%= ddlRating.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Details_RatingLabel %>" /></label>
                    <asp:DropDownList ID="ddlRating" runat="server">
                        <asp:ListItem Text="<%$ Resources:SiteText, Details_RatingChoose %>" Value="" />
                        <asp:ListItem Text="★★★★★ (5)" Value="5" />
                        <asp:ListItem Text="★★★★☆ (4)" Value="4" />
                        <asp:ListItem Text="★★★☆☆ (3)" Value="3" />
                        <asp:ListItem Text="★★☆☆☆ (2)" Value="2" />
                        <asp:ListItem Text="★☆☆☆☆ (1)" Value="1" />
                    </asp:DropDownList>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlRating" ErrorMessage="<%$ Resources:SiteText, Details_RatingChoose %>" Display="Dynamic" CssClass="field-error" />
                </div>
                <div class="form-row">
                    <label for="<%= txtComment.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Details_CommentLabel %>" /></label>
                    <asp:TextBox ID="txtComment" runat="server" TextMode="MultiLine" Rows="3" MaxLength="1000" />
                </div>
                <div class="form-row">
                    <asp:Button ID="btnSubmitReview" runat="server" Text="<%$ Resources:SiteText, Details_BtnSubmitReview %>" OnClick="btnSubmitReview_Click" CssClass="btn-primary" />
                </div>
            </asp:Panel>
        </div>
    </asp:Panel>
</asp:Content>
