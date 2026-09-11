<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="AdminLog.aspx.vb" Inherits="SumyPortal.AdminLog" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Журнал дій адміністратора — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Журнал дій адміністратора</h1>
    <p class="stub-note">
        <a href="~/AdminDashboard.aspx" runat="server">← Адмін-панель</a> ·
        <a href="~/AdminModeration.aspx" runat="server">Модерація</a> ·
        <a href="~/AdminServices.aspx" runat="server">Усі оголошення</a> ·
        <a href="~/AdminCategories.aspx" runat="server">Категорії</a> ·
        <a href="~/AdminUsers.aspx" runat="server">Користувачі</a>
    </p>

    <asp:Panel ID="emptyPanel" runat="server" Visible="false" CssClass="stub-note">
        Журнал поки порожній.
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
</asp:Content>
