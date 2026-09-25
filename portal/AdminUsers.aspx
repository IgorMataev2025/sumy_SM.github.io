<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="AdminUsers.aspx.vb" Inherits="SumyPortal.AdminUsers" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    <asp:Literal runat="server" Text="<%$ Resources:SiteText, PageTitle_AdminUsers %>" /> — <asp:Literal runat="server" Text="<%$ Resources:SiteText, SiteTitleSuffix %>" />
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Користувачі</h1>
    <p class="stub-note">
        <a href="~/AdminDashboard.aspx" runat="server">← Адмін-панель</a> ·
        <a href="~/AdminModeration.aspx" runat="server">Модерація</a> ·
        <a href="~/AdminServices.aspx" runat="server">Усі оголошення</a> ·
        <a href="~/AdminReports.aspx" runat="server">Скарги</a> ·
        <a href="~/AdminReviews.aspx" runat="server">Відгуки</a> ·
        <a href="~/AdminCategories.aspx" runat="server">Категорії</a> ·
        <a href="~/AdminDonations.aspx" runat="server">Донати</a> ·
        <a href="~/AdminLog.aspx" runat="server">Журнал дій</a> ·
        <a href="~/AdminOnlineUsers.aspx" runat="server">Онлайн</a>
    </p>

    <asp:Label ID="infoLabel" runat="server" CssClass="stub-note" Visible="false" />
    <%-- Пошук і фільтри (2026-09-25, аудит Адміна, п.4) — стан у адресі (AdminPageBase). --%>
    <asp:Panel runat="server" CssClass="filter-panel" DefaultButton="btnSearch">
        <div class="filter-row">
            <div class="form-row">
                <label for="<%= txtSearch.ClientID %>">Пошук</label>
                <asp:TextBox ID="txtSearch" runat="server" placeholder="email, ПІБ, компанія, телефон" />
            </div>
            <div class="form-row">
                <label for="<%= ddlRole.ClientID %>">Роль</label>
                <asp:DropDownList ID="ddlRole" runat="server">
                    <asp:ListItem Text="Усі" Value="" />
                    <asp:ListItem Text="Постачальники" Value="Provider" />
                    <asp:ListItem Text="Споживачі" Value="Consumer" />
                    <asp:ListItem Text="Адміністратори" Value="Admin" />
                </asp:DropDownList>
            </div>
            <div class="form-row">
                <label for="<%= ddlStatus.ClientID %>">Статус</label>
                <asp:DropDownList ID="ddlStatus" runat="server">
                    <asp:ListItem Text="Усі" Value="" />
                    <asp:ListItem Text="Активні" Value="active" />
                    <asp:ListItem Text="Заблоковані" Value="blocked" />
                </asp:DropDownList>
            </div>
            <div class="form-row filter-actions">
                <asp:Button ID="btnSearch" runat="server" Text="Знайти" OnClick="btnSearch_Click" CssClass="btn-primary" CausesValidation="false" />
                <a href="AdminUsers.aspx" class="btn-secondary btn-link">Скинути</a>
            </div>
        </div>
    </asp:Panel>
    <asp:Label ID="pageInfoLabel" runat="server" CssClass="page-info" />

    <div class="table-scroll">
        <table class="admin-table">
            <thead>
                <tr>
                    <th>Email</th>
                    <th>ПІБ</th>
                    <th>Тип</th>
                    <th>Статус</th>
                    <th>Оголошень</th>
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
                            <td><%#: If(CType(Container.DataItem, SumyPortal.UserAccount).UserType = "Provider", CType(Container.DataItem, SumyPortal.UserAccount).ServiceCount.ToString(), "—") %></td>
                            <td><%#: CType(Container.DataItem, SumyPortal.UserAccount).CreatedAt.ToString("dd.MM.yyyy") %></td>
                            <td>
                                <a href='<%#: "AdminUserEdit.aspx?id=" & CType(Container.DataItem, SumyPortal.UserAccount).UserId %>'>Редагувати</a>
                                <asp:LinkButton runat="server" CommandName="ToggleActive"
                                    CommandArgument='<%#: CType(Container.DataItem, SumyPortal.UserAccount).UserId %>'
                                    Visible='<%#: Not CType(Container.DataItem, SumyPortal.UserAccount).IsAdmin %>'>
                                    · <%#: If(CType(Container.DataItem, SumyPortal.UserAccount).IsActive, "Заблокувати", "Розблокувати") %>
                                </asp:LinkButton>
                                <asp:LinkButton runat="server" CommandName="Delete" CausesValidation="false"
                                    CommandArgument='<%#: CType(Container.DataItem, SumyPortal.UserAccount).UserId %>'
                                    Visible='<%#: Not CType(Container.DataItem, SumyPortal.UserAccount).IsAdmin %>'
                                    OnClientClick="return confirm('Видалити цього користувача та всі його оголошення назавжди? Дію не можна скасувати.');">
                                    · Видалити
                                </asp:LinkButton>
                            </td>
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
