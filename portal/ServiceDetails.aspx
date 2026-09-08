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
            <p>
                <asp:Button ID="btnToggleFavorite" runat="server" Visible="false" CausesValidation="false"
                    OnClick="btnToggleFavorite_Click" CssClass="btn-secondary" />
            </p>
        </div>

        <div class="reviews-section">
            <h2>Рейтинг та відгуки</h2>
            <p class="rating-summary"><asp:Literal ID="ratingSummaryLiteral" runat="server" /></p>

            <asp:Repeater ID="rptReviews" runat="server">
                <ItemTemplate>
                    <div class="review-card">
                        <p class="review-meta">
                            <b><%#: CType(Container.DataItem, SumyPortal.Review).ConsumerName %></b> ·
                            <%#: New String("★"c, CType(Container.DataItem, SumyPortal.Review).Rating) & New String("☆"c, 5 - CType(Container.DataItem, SumyPortal.Review).Rating) %> ·
                            <%#: CType(Container.DataItem, SumyPortal.Review).CreatedAt.ToString("dd.MM.yyyy") %>
                        </p>
                        <asp:Literal runat="server" Visible='<%#: Not String.IsNullOrEmpty(CType(Container.DataItem, SumyPortal.Review).Comment) %>'
                            Text='<%#: CType(Container.DataItem, SumyPortal.Review).Comment %>' />
                    </div>
                </ItemTemplate>
            </asp:Repeater>

            <asp:Panel ID="noReviewsPanel" runat="server" Visible="false" CssClass="stub-note">
                Відгуків ще немає.
            </asp:Panel>

            <asp:Panel ID="alreadyReviewedPanel" runat="server" Visible="false" CssClass="stub-note">
                Ви вже залишили відгук на це оголошення.
            </asp:Panel>

            <asp:Panel ID="reviewFormPanel" runat="server" Visible="false" CssClass="auth-form">
                <h3>Залишити відгук</h3>
                <asp:ValidationSummary ID="reviewValidationSummary" runat="server" CssClass="form-error" DisplayMode="BulletList" />
                <asp:Label ID="reviewErrorLabel" runat="server" CssClass="form-error" Visible="false" />
                <div class="form-row">
                    <label for="<%= ddlRating.ClientID %>">Оцінка</label>
                    <asp:DropDownList ID="ddlRating" runat="server">
                        <asp:ListItem Text="Оберіть оцінку" Value="" />
                        <asp:ListItem Text="★★★★★ (5)" Value="5" />
                        <asp:ListItem Text="★★★★☆ (4)" Value="4" />
                        <asp:ListItem Text="★★★☆☆ (3)" Value="3" />
                        <asp:ListItem Text="★★☆☆☆ (2)" Value="2" />
                        <asp:ListItem Text="★☆☆☆☆ (1)" Value="1" />
                    </asp:DropDownList>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlRating" ErrorMessage="Оберіть оцінку" Display="Dynamic" CssClass="field-error" />
                </div>
                <div class="form-row">
                    <label for="<%= txtComment.ClientID %>">Коментар (необов'язково)</label>
                    <asp:TextBox ID="txtComment" runat="server" TextMode="MultiLine" Rows="3" MaxLength="1000" />
                </div>
                <div class="form-row">
                    <asp:Button ID="btnSubmitReview" runat="server" Text="Залишити відгук" OnClick="btnSubmitReview_Click" CssClass="btn-primary" />
                </div>
            </asp:Panel>
        </div>
    </asp:Panel>
</asp:Content>
