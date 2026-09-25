<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="AdminDonations.aspx.vb" Inherits="SumyPortal.AdminDonations" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    <asp:Literal runat="server" Text="<%$ Resources:SiteText, PageTitle_AdminDonations %>" /> — <asp:Literal runat="server" Text="<%$ Resources:SiteText, SiteTitleSuffix %>" />
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Донати</h1>
    <p class="stub-note">
        <a href="~/AdminDashboard.aspx" runat="server">← Адмін-панель</a> ·
        <a href="~/AdminModeration.aspx" runat="server">Модерація</a> ·
        <a href="~/AdminServices.aspx" runat="server">Усі оголошення</a> ·
        <a href="~/AdminReports.aspx" runat="server">Скарги</a> ·
        <a href="~/AdminReviews.aspx" runat="server">Відгуки</a> ·
        <a href="~/AdminCategories.aspx" runat="server">Категорії</a> ·
        <a href="~/AdminUsers.aspx" runat="server">Користувачі</a> ·
        <a href="~/AdminLog.aspx" runat="server">Журнал дій</a> ·
        <a href="~/AdminOnlineUsers.aspx" runat="server">Онлайн</a>
    </p>

    <%-- Донати LiqPay (2026-09-25, аудит Адміна, п.6) — лише читання; суми — лише оплачені. --%>
    <p class="stub-note">
        Оплачено за 30 днів: <b><asp:Literal ID="sum30Literal" runat="server" /> грн</b> ·
        за весь час: <b><asp:Literal ID="sumAllLiteral" runat="server" /> грн</b>
    </p>

    <asp:Panel runat="server" CssClass="filter-panel" DefaultButton="btnSearch">
        <div class="filter-row">
            <div class="form-row">
                <label for="<%= ddlStatus.ClientID %>">Статус</label>
                <asp:DropDownList ID="ddlStatus" runat="server">
                    <asp:ListItem Text="Усі" Value="" />
                    <asp:ListItem Text="Оплачено" Value="Success" />
                    <asp:ListItem Text="Очікує" Value="Pending" />
                    <asp:ListItem Text="Не вдалося" Value="Failure" />
                </asp:DropDownList>
            </div>
            <div class="form-row filter-actions">
                <asp:Button ID="btnSearch" runat="server" Text="Показати" OnClick="btnSearch_Click" CssClass="btn-primary" CausesValidation="false" />
            </div>
        </div>
    </asp:Panel>
    <asp:Label ID="pageInfoLabel" runat="server" CssClass="page-info" />

    <asp:Panel ID="emptyPanel" runat="server" Visible="false" CssClass="stub-note">
        Донатів немає.
    </asp:Panel>

    <div class="table-scroll">
        <table class="admin-table">
            <thead>
                <tr><th>Дата (UTC)</th><th>Сума</th><th>Статус</th><th>Донатор</th><th>Замовлення</th></tr>
            </thead>
            <tbody>
                <asp:Repeater ID="rptDonations" runat="server">
                    <ItemTemplate>
                        <tr>
                            <td><%#: CType(Container.DataItem, SumyPortal.AdminDonation).CreatedAt.ToString("dd.MM.yyyy HH:mm") %></td>
                            <td><%#: CType(Container.DataItem, SumyPortal.AdminDonation).Amount.ToString("0.##") %> <%#: CType(Container.DataItem, SumyPortal.AdminDonation).Currency %></td>
                            <td><%#: CType(Container.DataItem, SumyPortal.AdminDonation).StatusLabel %></td>
                            <td><%#: If(String.IsNullOrEmpty(CType(Container.DataItem, SumyPortal.AdminDonation).DonorName), "анонімно", CType(Container.DataItem, SumyPortal.AdminDonation).DonorName) %>
                                <%#: If(String.IsNullOrEmpty(CType(Container.DataItem, SumyPortal.AdminDonation).DonorEmail), "", " (" & CType(Container.DataItem, SumyPortal.AdminDonation).DonorEmail & ")") %></td>
                            <td><%#: CType(Container.DataItem, SumyPortal.AdminDonation).OrderId %></td>
                        </tr>
                    </ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>
    </div>
    <div class="pagination">
        <asp:HyperLink ID="lnkPrev" runat="server" Text="← Попередня" />
        <asp:HyperLink ID="lnkNext" runat="server" Text="Наступна →" />
    </div>
</asp:Content>
