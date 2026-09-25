<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="AdminReviews.aspx.vb" Inherits="SumyPortal.AdminReviews" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    <asp:Literal runat="server" Text="<%$ Resources:SiteText, PageTitle_AdminReviews %>" /> — <asp:Literal runat="server" Text="<%$ Resources:SiteText, SiteTitleSuffix %>" />
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Останні відгуки</h1>
    <p class="stub-note">
        <a href="~/AdminDashboard.aspx" runat="server">← Адмін-панель</a> ·
        <a href="~/AdminModeration.aspx" runat="server">Модерація</a> ·
        <a href="~/AdminServices.aspx" runat="server">Усі оголошення</a> ·
        <a href="~/AdminReports.aspx" runat="server">Скарги</a> ·
        <a href="~/AdminCategories.aspx" runat="server">Категорії</a> ·
        <a href="~/AdminUsers.aspx" runat="server">Користувачі</a> ·
        <a href="~/AdminDonations.aspx" runat="server">Донати</a> ·
        <a href="~/AdminLog.aspx" runat="server">Журнал дій</a> ·
        <a href="~/AdminOnlineUsers.aspx" runat="server">Онлайн</a>
    </p>

    <%-- Загальний список відгуків (2026-09-25, аудит Адміна, п.7) — швидко прибрати спам/образи,
         не відкриваючи кожне оголошення (раніше видалення було лише в AdminServiceEdit.aspx). --%>
    <asp:Label ID="infoLabel" runat="server" CssClass="stub-note" Visible="false" />

    <asp:Panel runat="server" CssClass="filter-panel" DefaultButton="btnSearch">
        <div class="filter-row">
            <div class="form-row">
                <label for="<%= ddlRating.ClientID %>">Оцінка</label>
                <asp:DropDownList ID="ddlRating" runat="server">
                    <asp:ListItem Text="Усі" Value="" />
                    <asp:ListItem Text="★★ і нижче (негативні)" Value="2" />
                    <asp:ListItem Text="★★★ і нижче" Value="3" />
                </asp:DropDownList>
            </div>
            <div class="form-row filter-actions">
                <asp:Button ID="btnSearch" runat="server" Text="Показати" OnClick="btnSearch_Click" CssClass="btn-primary" CausesValidation="false" />
            </div>
        </div>
    </asp:Panel>
    <asp:Label ID="pageInfoLabel" runat="server" CssClass="page-info" />

    <asp:Panel ID="emptyPanel" runat="server" Visible="false" CssClass="stub-note">
        Відгуків немає.
    </asp:Panel>

    <asp:Repeater ID="rptReviews" runat="server" OnItemCommand="rptReviews_ItemCommand">
        <ItemTemplate>
            <div class="service-card">
                <div class="service-card-header">
                    <h3><%#: New String("★"c, CType(Container.DataItem, SumyPortal.AdminReviewRow).Rating) & New String("☆"c, 5 - CType(Container.DataItem, SumyPortal.AdminReviewRow).Rating) %>
                        — <%#: CType(Container.DataItem, SumyPortal.AdminReviewRow).ServiceTitle %></h3>
                    <span class="status-badge"><%#: CType(Container.DataItem, SumyPortal.AdminReviewRow).CreatedAt.ToString("dd.MM.yyyy HH:mm") %></span>
                </div>
                <p class="service-category">
                    <%#: CType(Container.DataItem, SumyPortal.AdminReviewRow).ConsumerName %> (<%#: CType(Container.DataItem, SumyPortal.AdminReviewRow).ConsumerEmail %>)
                </p>
                <p><%#: CType(Container.DataItem, SumyPortal.AdminReviewRow).Comment %></p>
                <div class="service-card-actions">
                    <a href='<%#: "ServiceDetails.aspx?id=" & CType(Container.DataItem, SumyPortal.AdminReviewRow).ServiceId %>' target="_blank">Оголошення</a>
                    <asp:LinkButton runat="server" CommandName="Delete"
                        CommandArgument='<%#: CType(Container.DataItem, SumyPortal.AdminReviewRow).ReviewId %>'
                        OnClientClick="return confirm('Видалити цей відгук назавжди?');">
                        ✕ Видалити відгук
                    </asp:LinkButton>
                </div>
            </div>
        </ItemTemplate>
    </asp:Repeater>
    <div class="pagination">
        <asp:HyperLink ID="lnkPrev" runat="server" Text="← Попередня" />
        <asp:HyperLink ID="lnkNext" runat="server" Text="Наступна →" />
    </div>
</asp:Content>
