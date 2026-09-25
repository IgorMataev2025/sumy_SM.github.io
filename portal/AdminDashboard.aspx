<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="AdminDashboard.aspx.vb" Inherits="SumyPortal.AdminDashboard" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    <asp:Literal runat="server" Text="<%$ Resources:SiteText, PageTitle_AdminDashboard %>" /> — <asp:Literal runat="server" Text="<%$ Resources:SiteText, SiteTitleSuffix %>" />
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Адмін-панель</h1>
    <p class="stub-note">
        Ця сторінка не пов'язана з головним меню — збережіть її в закладках,
        щоб повертатись напряму.
    </p>

    <div class="categories">
        <a class="category-card" href="~/AdminModeration.aspx" runat="server">
            <h3>Модерація оголошень</h3>
            <p><asp:Literal ID="pendingCountLiteral" runat="server" /></p>
        </a>
        <a class="category-card" href="~/AdminServices.aspx" runat="server">
            <h3>Усі оголошення</h3>
            <p><asp:Literal ID="totalServicesCountLiteral" runat="server" /></p>
        </a>
        <a class="category-card" href="~/AdminReports.aspx" runat="server">
            <h3>Скарги на оголошення</h3>
            <p><asp:Literal ID="reportsCountLiteral" runat="server" /></p>
        </a>
        <a class="category-card" href="~/AdminCategories.aspx" runat="server">
            <h3>Категорії послуг</h3>
            <p><asp:Literal ID="categoryCountLiteral" runat="server" /></p>
        </a>
        <a class="category-card" href="~/AdminUsers.aspx" runat="server">
            <h3>Користувачі</h3>
            <p><asp:Literal ID="userCountLiteral" runat="server" /></p>
        </a>
        <a class="category-card" href="~/AdminLog.aspx" runat="server">
            <h3>Журнал дій</h3>
            <p><asp:Literal ID="logCountLiteral" runat="server" /></p>
        </a>
        <a class="category-card" href="~/AdminOnlineUsers.aspx" runat="server">
            <h3>Онлайн зараз</h3>
            <p><asp:Literal ID="onlineCountLiteral" runat="server" /></p>
        </a>
        <a class="category-card" href="~/AdminReviews.aspx" runat="server">
            <h3>Відгуки</h3>
            <p><asp:Literal ID="reviewsLiteral" runat="server" /></p>
        </a>
        <a class="category-card" href="~/AdminDonations.aspx" runat="server">
            <h3>Донати</h3>
            <p><asp:Literal ID="donationsLiteral" runat="server" /></p>
        </a>
    </div>

    <%-- Динаміка (2026-09-25, аудит Адміна, п.8): за 7 і 30 днів та за весь час. --%>
    <h2>Статистика</h2>
    <div class="table-scroll">
        <table class="admin-table">
            <thead>
                <tr><th>Показник</th><th>7 днів</th><th>30 днів</th><th>Усього</th></tr>
            </thead>
            <tbody>
                <asp:Repeater ID="rptStats" runat="server">
                    <ItemTemplate>
                        <tr>
                            <td><%#: CType(Container.DataItem, SumyPortal.AdminStatRow).Label %></td>
                            <td><%#: CType(Container.DataItem, SumyPortal.AdminStatRow).Last7 %></td>
                            <td><%#: CType(Container.DataItem, SumyPortal.AdminStatRow).Last30 %></td>
                            <td><%#: CType(Container.DataItem, SumyPortal.AdminStatRow).Total %></td>
                        </tr>
                    </ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>
    </div>
</asp:Content>
