<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="AdminServices.aspx.vb" Inherits="SumyPortal.AdminServices" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Усі оголошення — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Усі оголошення</h1>
    <p class="stub-note">
        <a href="~/AdminDashboard.aspx" runat="server">← Адмін-панель</a> ·
        <a href="~/AdminModeration.aspx" runat="server">Модерація</a> ·
        <a href="~/AdminCategories.aspx" runat="server">Категорії</a> ·
        <a href="~/AdminUsers.aspx" runat="server">Користувачі</a> ·
        <a href="~/AdminLog.aspx" runat="server">Журнал дій</a>
    </p>
    <p class="stub-note">
        Прямий CRUD адміністратора над будь-яким оголошенням (п.12 уточненої
        постановки) — на відміну від «Модерації» тут можна редагувати/видаляти
        оголошення в будь-якому статусі, не лише схвалювати/відхиляти чергу.
    </p>

    <asp:Label ID="infoLabel" runat="server" CssClass="stub-note" Visible="false" />

    <div class="filter-panel">
        <div class="form-row">
            <label for="<%= ddlStatusFilter.ClientID %>">Статус</label>
            <asp:DropDownList ID="ddlStatusFilter" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlStatusFilter_SelectedIndexChanged">
                <asp:ListItem Text="Усі статуси" Value="" />
                <asp:ListItem Text="Чернетка" Value="Draft" />
                <asp:ListItem Text="На модерації" Value="Pending" />
                <asp:ListItem Text="Опубліковано" Value="Approved" />
                <asp:ListItem Text="Відхилено" Value="Rejected" />
            </asp:DropDownList>
        </div>
    </div>

    <asp:Panel ID="emptyPanel" runat="server" Visible="false" CssClass="stub-note">
        Оголошень із таким статусом немає.
    </asp:Panel>

    <asp:Repeater ID="rptServices" runat="server" OnItemCommand="rptServices_ItemCommand">
        <ItemTemplate>
            <div class="service-card status-<%#: CType(Container.DataItem, SumyPortal.Service).Status.ToLowerInvariant() %>">
                <div class="service-card-header">
                    <h3><%#: CType(Container.DataItem, SumyPortal.Service).Title %></h3>
                    <span class="status-badge"><%#: CType(Container.DataItem, SumyPortal.Service).StatusLabel %></span>
                </div>
                <p class="service-category">
                    <%#: CType(Container.DataItem, SumyPortal.Service).CategoryName %> ·
                    <%#: CType(Container.DataItem, SumyPortal.Service).ProviderName %>
                    (<%#: CType(Container.DataItem, SumyPortal.Service).ProviderEmail %>) ·
                    <%#: CType(Container.DataItem, SumyPortal.Service).CreatedAt.ToString("dd.MM.yyyy") %>
                </p>

                <div class="service-card-actions">
                    <a href='<%#: "AdminServiceEdit.aspx?id=" & CType(Container.DataItem, SumyPortal.Service).ServiceId %>'>Редагувати</a>
                    <asp:LinkButton runat="server" CommandName="Delete"
                        CommandArgument='<%#: CType(Container.DataItem, SumyPortal.Service).ServiceId %>'
                        OnClientClick="return confirm('Видалити це оголошення назавжди? Дію не можна скасувати.');">
                        Видалити
                    </asp:LinkButton>
                </div>
            </div>
        </ItemTemplate>
    </asp:Repeater>
</asp:Content>
