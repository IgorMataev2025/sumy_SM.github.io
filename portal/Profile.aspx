<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Profile.aspx.vb" Inherits="SumyPortal.Profile" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Мій профіль — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Мій профіль</h1>

    <asp:Label ID="infoLabel" runat="server" CssClass="stub-note" Visible="false" />

    <asp:Panel ID="formPanel" runat="server" CssClass="auth-form">
        <asp:ValidationSummary ID="validationSummary" runat="server" CssClass="form-error" DisplayMode="BulletList" />
        <asp:Label ID="serverErrorLabel" runat="server" CssClass="form-error" Visible="false" />

        <div class="form-row">
            <label>Email</label>
            <asp:Literal ID="emailLiteral" runat="server" />
        </div>

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

    <asp:Panel ID="dangerZonePanel" runat="server" CssClass="stub-note danger-zone">
        <h2>Видалення акаунта</h2>
        <p>
            Видалення знеособлює ваші дані (ПІБ, телефон, email, реквізити) і
            блокує вхід; ваші опубліковані оголошення знімаються з публікації.
            Дію не можна скасувати самостійно — див.
            <asp:HyperLink runat="server" NavigateUrl="~/PrivacyPolicy.aspx" Target="_blank">Політику конфіденційності</asp:HyperLink>.
        </p>
        <asp:Button ID="btnDeleteAccount" runat="server" Text="Видалити акаунт" CssClass="btn-secondary"
            OnClick="btnDeleteAccount_Click" CausesValidation="false"
            OnClientClick="return confirm('Видалити акаунт безповоротно? Цю дію не можна скасувати самостійно.');" />
    </asp:Panel>
</asp:Content>
