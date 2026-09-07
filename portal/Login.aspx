<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Login.aspx.vb" Inherits="SumyPortal.Login" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Вхід — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Вхід</h1>

    <asp:Panel ID="formPanel" runat="server" CssClass="auth-form">
        <asp:Label ID="serverErrorLabel" runat="server" CssClass="form-error" Visible="false" />

        <div class="form-row">
            <label for="<%= txtEmail.ClientID %>">Email</label>
            <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" MaxLength="255" />
            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmail" ErrorMessage="Вкажіть email" Display="Dynamic" CssClass="field-error" />
        </div>

        <div class="form-row">
            <label for="<%= txtPassword.ClientID %>">Пароль</label>
            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" MaxLength="100" />
            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPassword" ErrorMessage="Вкажіть пароль" Display="Dynamic" CssClass="field-error" />
        </div>

        <div class="form-row">
            <asp:Button ID="btnLogin" runat="server" Text="Увійти" OnClick="btnLogin_Click" CssClass="btn-primary" />
        </div>

        <p><asp:HyperLink ID="forgotPasswordLink" runat="server" NavigateUrl="~/ForgotPassword.aspx">Забули пароль?</asp:HyperLink></p>
        <p>Ще немає акаунта? <asp:HyperLink runat="server" NavigateUrl="~/Register.aspx">Зареєструватися</asp:HyperLink></p>
    </asp:Panel>
</asp:Content>
