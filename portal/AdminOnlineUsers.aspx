<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="AdminOnlineUsers.aspx.vb" Inherits="SumyPortal.AdminOnlineUsers" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    <asp:Literal runat="server" Text="<%$ Resources:SiteText, PageTitle_AdminOnlineUsers %>" /> — <asp:Literal runat="server" Text="<%$ Resources:SiteText, SiteTitleSuffix %>" />
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <meta http-equiv="refresh" content="20" />
    <h1>Онлайн зараз</h1>
    <p class="stub-note">
        <a href="~/AdminDashboard.aspx" runat="server">← Адмін-панель</a> ·
        <a href="~/AdminModeration.aspx" runat="server">Модерація</a> ·
        <a href="~/AdminServices.aspx" runat="server">Усі оголошення</a> ·
        <a href="~/AdminReports.aspx" runat="server">Скарги</a> ·
        <a href="~/AdminReviews.aspx" runat="server">Відгуки</a> ·
        <a href="~/AdminCategories.aspx" runat="server">Категорії</a> ·
        <a href="~/AdminUsers.aspx" runat="server">Користувачі</a> ·
        <a href="~/AdminDonations.aspx" runat="server">Донати</a> ·
        <a href="~/AdminLog.aspx" runat="server">Журнал дій</a>
    </p>
    <p class="stub-note">
        <asp:Literal ID="thresholdLiteral" runat="server" /> Сторінка сама оновлюється
        кожні 20 секунд — оновлено востаннє: <asp:Literal ID="refreshedAtLiteral" runat="server" />.
    </p>

    <asp:Panel ID="emptyPanel" runat="server" Visible="false" CssClass="stub-note">
        Зараз онлайн немає жодного користувача.
    </asp:Panel>

    <div class="table-scroll">
        <table class="admin-table">
            <thead>
                <tr>
                    <th>Email</th>
                    <th>ПІБ / Компанія</th>
                    <th>Тип</th>
                    <th>Остання активність</th>
                </tr>
            </thead>
            <tbody>
                <asp:Repeater ID="rptOnlineUsers" runat="server">
                    <ItemTemplate>
                        <tr>
                            <td><%#: CType(Container.DataItem, SumyPortal.UserAccount).Email %></td>
                            <td><%#: SumyPortal.AdminOnlineUsers.DisplayName(CType(Container.DataItem, SumyPortal.UserAccount)) %></td>
                            <td>
                                <%#: If(CType(Container.DataItem, SumyPortal.UserAccount).UserType = "Provider", "Постачальник", "Споживач") %>
                                <%#: If(CType(Container.DataItem, SumyPortal.UserAccount).IsAdmin, " (адмін)", "") %>
                            </td>
                            <td><%#: SumyPortal.AdminOnlineUsers.FormatMinutesAgo(CType(Container.DataItem, SumyPortal.UserAccount).LastActivityAt) %></td>
                        </tr>
                    </ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>
    </div>
</asp:Content>
