<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="AdminCategories.aspx.vb" Inherits="SumyPortal.AdminCategories" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Категорії — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Категорії послуг</h1>
    <p class="stub-note">
        <a href="~/AdminDashboard.aspx" runat="server">← Адмін-панель</a> ·
        <a href="~/AdminModeration.aspx" runat="server">Модерація</a> ·
        <a href="~/AdminUsers.aspx" runat="server">Користувачі</a> ·
        <a href="~/AdminLog.aspx" runat="server">Журнал дій</a>
    </p>

    <asp:Label ID="infoLabel" runat="server" CssClass="stub-note" Visible="false" />

    <asp:Panel ID="formPanel" runat="server" CssClass="auth-form">
        <h2><asp:Literal ID="formHeading" runat="server" Text="Нова категорія" /></h2>
        <asp:Label ID="serverErrorLabel" runat="server" CssClass="form-error" Visible="false" />

        <div class="form-row">
            <label for="<%= txtName.ClientID %>">Назва</label>
            <asp:TextBox ID="txtName" runat="server" MaxLength="150" />
            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtName" ErrorMessage="Вкажіть назву" Display="Dynamic" CssClass="field-error" />
        </div>
        <div class="form-row">
            <label for="<%= txtDescription.ClientID %>">Опис (короткий підпис картки)</label>
            <asp:TextBox ID="txtDescription" runat="server" MaxLength="500" />
        </div>
        <div class="form-row form-actions">
            <asp:Button ID="btnSave" runat="server" Text="Зберегти" OnClick="btnSave_Click" CssClass="btn-primary" />
            <asp:Button ID="btnCancel" runat="server" Text="Скасувати редагування" OnClick="btnCancel_Click" CausesValidation="false" CssClass="btn-secondary" Visible="false" />
        </div>
    </asp:Panel>

    <asp:Repeater ID="rptCategories" runat="server" OnItemCommand="rptCategories_ItemCommand">
        <ItemTemplate>
            <div class="service-card <%#: If(CType(Container.DataItem, SumyPortal.ServiceCategory).IsActive, "status-approved", "status-rejected") %>">
                <div class="service-card-header">
                    <h3><%#: CType(Container.DataItem, SumyPortal.ServiceCategory).Name %></h3>
                    <span class="status-badge"><%#: If(CType(Container.DataItem, SumyPortal.ServiceCategory).IsActive, "Активна", "Деактивована") %></span>
                </div>
                <p class="service-category"><%#: CType(Container.DataItem, SumyPortal.ServiceCategory).Description %></p>
                <div class="service-card-actions">
                    <asp:LinkButton runat="server" CommandName="Edit"
                        CommandArgument='<%#: CType(Container.DataItem, SumyPortal.ServiceCategory).CategoryId %>'>Редагувати</asp:LinkButton>
                    <asp:LinkButton runat="server" CommandName="ToggleActive"
                        CommandArgument='<%#: CType(Container.DataItem, SumyPortal.ServiceCategory).CategoryId %>'>
                        <%#: If(CType(Container.DataItem, SumyPortal.ServiceCategory).IsActive, "Деактивувати", "Активувати") %>
                    </asp:LinkButton>
                </div>
            </div>
        </ItemTemplate>
    </asp:Repeater>
</asp:Content>
