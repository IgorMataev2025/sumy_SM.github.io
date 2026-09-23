<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="AdminUserEdit.aspx.vb" Inherits="SumyPortal.AdminUserEdit" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    <asp:Literal runat="server" Text="<%$ Resources:SiteText, PageTitle_AdminUserEdit %>" /> — <asp:Literal runat="server" Text="<%$ Resources:SiteText, SiteTitleSuffix %>" />
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Редагування користувача</h1>
    <p class="stub-note">
        <a href="~/AdminUsers.aspx" runat="server">← Користувачі</a> ·
        <asp:Literal ID="emailLiteral" runat="server" />
    </p>

    <asp:Panel ID="notFoundPanel" runat="server" Visible="false" CssClass="stub-note">
        Користувача не знайдено.
    </asp:Panel>

    <asp:Panel ID="formPanel" runat="server" CssClass="auth-form">
        <asp:ValidationSummary ID="validationSummary" runat="server" CssClass="form-error" DisplayMode="BulletList" />
        <asp:Label ID="serverErrorLabel" runat="server" CssClass="form-error" Visible="false" />
        <asp:Label ID="infoLabel" runat="server" CssClass="stub-note" Visible="false" />

        <div class="form-row">
            <label for="<%= txtFullName.ClientID %>">ПІБ</label>
            <asp:TextBox ID="txtFullName" runat="server" MaxLength="255" />
            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtFullName" ErrorMessage="Вкажіть ПІБ" Display="Dynamic" CssClass="field-error" />
        </div>

        <div class="form-row">
            <label for="<%= txtPhone.ClientID %>">Телефон</label>
            <asp:TextBox ID="txtPhone" runat="server" MaxLength="50" />
        </div>

        <div class="form-row">
            <label for="<%= ddlDistrict.ClientID %>">Район</label>
            <asp:DropDownList ID="ddlDistrict" runat="server" />
        </div>

        <asp:Panel ID="legalEntityPanel" runat="server" Visible="false">
            <div class="form-row">
                <label for="<%= txtCompanyName.ClientID %>">Назва компанії</label>
                <asp:TextBox ID="txtCompanyName" runat="server" MaxLength="255" />
            </div>
            <div class="form-row">
                <label for="<%= txtEdrpou.ClientID %>">ЄДРПОУ</label>
                <asp:TextBox ID="txtEdrpou" runat="server" MaxLength="20" />
                <asp:RegularExpressionValidator runat="server" ControlToValidate="txtEdrpou" ErrorMessage="ЄДРПОУ — 8 цифр" ValidationExpression="^\d{8}$" Display="Dynamic" CssClass="field-error" />
            </div>
        </asp:Panel>

        <div class="form-row">
            <asp:Button ID="btnSave" runat="server" Text="Зберегти" OnClick="btnSave_Click" CssClass="btn-primary" />
        </div>
    </asp:Panel>
</asp:Content>
