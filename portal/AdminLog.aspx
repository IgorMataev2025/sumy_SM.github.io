<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="AdminLog.aspx.vb" Inherits="SumyPortal.AdminLog" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    <asp:Literal runat="server" Text="<%$ Resources:SiteText, PageTitle_AdminLog %>" /> — <asp:Literal runat="server" Text="<%$ Resources:SiteText, SiteTitleSuffix %>" />
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Журнал дій адміністратора</h1>
    <p class="stub-note">
        <a href="~/AdminDashboard.aspx" runat="server">← Адмін-панель</a> ·
        <a href="~/AdminModeration.aspx" runat="server">Модерація</a> ·
        <a href="~/AdminServices.aspx" runat="server">Усі оголошення</a> ·
        <a href="~/AdminReports.aspx" runat="server">Скарги</a> ·
        <a href="~/AdminReviews.aspx" runat="server">Відгуки</a> ·
        <a href="~/AdminCategories.aspx" runat="server">Категорії</a> ·
        <a href="~/AdminUsers.aspx" runat="server">Користувачі</a> ·
        <a href="~/AdminDonations.aspx" runat="server">Донати</a> ·
        <a href="~/AdminOnlineUsers.aspx" runat="server">Онлайн</a>
    </p>

    <%-- Фільтри й сторінки (2026-09-25, аудит Адміна, п.9) — стан у адресі (AdminPageBase). --%>
    <asp:Panel runat="server" CssClass="filter-panel" DefaultButton="btnSearch">
        <div class="filter-row">
            <div class="form-row">
                <label for="<%= txtSearch.ClientID %>">Пошук</label>
                <asp:TextBox ID="txtSearch" runat="server" placeholder="дія, деталі, email адміна" />
            </div>
            <div class="form-row">
                <label for="<%= txtFrom.ClientID %>">Від</label>
                <asp:TextBox ID="txtFrom" runat="server" TextMode="Date" />
            </div>
            <div class="form-row">
                <label for="<%= txtTo.ClientID %>">До</label>
                <asp:TextBox ID="txtTo" runat="server" TextMode="Date" />
            </div>
            <div class="form-row filter-actions">
                <asp:Button ID="btnSearch" runat="server" Text="Знайти" OnClick="btnSearch_Click" CssClass="btn-primary" CausesValidation="false" />
                <a href="AdminLog.aspx" class="btn-secondary btn-link">Скинути</a>
            </div>
        </div>
    </asp:Panel>
    <asp:Label ID="pageInfoLabel" runat="server" CssClass="page-info" />

    <asp:Panel ID="emptyPanel" runat="server" Visible="false" CssClass="stub-note">
        Записів за цим фільтром немає.
    </asp:Panel>

    <div class="table-scroll">
        <table class="admin-table">
            <thead>
                <tr>
                    <th>Дата</th>
                    <th>Адмін</th>
                    <th>Дія</th>
                    <th>Деталі</th>
                </tr>
            </thead>
            <tbody>
                <asp:Repeater ID="rptLog" runat="server">
                    <ItemTemplate>
                        <tr>
                            <td><%#: CType(Container.DataItem, SumyPortal.AdminLogEntry).ActionDate.ToString("dd.MM.yyyy HH:mm") %></td>
                            <td><%#: CType(Container.DataItem, SumyPortal.AdminLogEntry).AdminName %></td>
                            <td><%#: CType(Container.DataItem, SumyPortal.AdminLogEntry).Action %></td>
                            <td><%#: CType(Container.DataItem, SumyPortal.AdminLogEntry).Details %></td>
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
