<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="AdminUsers.aspx.vb" Inherits="SumyPortal.AdminUsers" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Користувачі — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Користувачі</h1>

    <asp:Label ID="infoLabel" runat="server" CssClass="stub-note" Visible="false" />

    <div class="table-scroll">
        <table class="admin-table">
            <thead>
                <tr>
                    <th>Email</th>
                    <th>ПІБ</th>
                    <th>Тип</th>
                    <th>Статус</th>
                    <th>Реєстрація</th>
                    <th></th>
                </tr>
            </thead>
            <tbody>
                <asp:Repeater ID="rptUsers" runat="server" OnItemCommand="rptUsers_ItemCommand">
                    <ItemTemplate>
                        <tr>
                            <td><%#: CType(Container.DataItem, SumyPortal.UserAccount).Email %></td>
                            <td><%#: CType(Container.DataItem, SumyPortal.UserAccount).FullName %></td>
                            <td>
                                <%#: If(CType(Container.DataItem, SumyPortal.UserAccount).UserType = "Provider", "Постачальник", "Споживач") %>
                                <%#: If(CType(Container.DataItem, SumyPortal.UserAccount).IsAdmin, " (адмін)", "") %>
                            </td>
                            <td><%#: If(CType(Container.DataItem, SumyPortal.UserAccount).IsActive, "Активний", "Заблокований") %></td>
                            <td><%#: CType(Container.DataItem, SumyPortal.UserAccount).CreatedAt.ToString("dd.MM.yyyy") %></td>
                            <td>
                                <asp:LinkButton runat="server" CommandName="ToggleActive"
                                    CommandArgument='<%#: CType(Container.DataItem, SumyPortal.UserAccount).UserId %>'
                                    Visible='<%#: Not CType(Container.DataItem, SumyPortal.UserAccount).IsAdmin %>'>
                                    <%#: If(CType(Container.DataItem, SumyPortal.UserAccount).IsActive, "Заблокувати", "Розблокувати") %>
                                </asp:LinkButton>
                            </td>
                        </tr>
                    </ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>
    </div>
</asp:Content>
