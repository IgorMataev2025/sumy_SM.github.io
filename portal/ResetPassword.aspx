<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="ResetPassword.aspx.vb" Inherits="SumyPortal.ResetPassword" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Новий пароль — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Literal runat="server" Text="<%$ Resources:SiteText, ResetPassword_Heading %>" /></h1>

    <asp:Panel ID="invalidTokenPanel" runat="server" Visible="false" CssClass="stub-note">
        <asp:Literal runat="server" Text="<%$ Resources:SiteText, ResetPassword_InvalidToken_Prefix %>" />
        <a href="ForgotPassword.aspx"><asp:Literal runat="server" Text="<%$ Resources:SiteText, ResetPassword_InvalidToken_Link %>" /></a>.
    </asp:Panel>

    <asp:Panel ID="successPanel" runat="server" Visible="false" CssClass="stub-note">
        <asp:Literal runat="server" Text="<%$ Resources:SiteText, ResetPassword_Success_Prefix %>" />
        <a href="Login.aspx"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Details_AnonContact_Login %>" /></a>.
    </asp:Panel>

    <asp:Panel ID="formPanel" runat="server" CssClass="auth-form">
        <asp:Label ID="serverErrorLabel" runat="server" CssClass="form-error" Visible="false" />
        <div class="form-row">
            <label for="<%= txtPassword.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:SiteText, ResetPassword_Label_NewPassword %>" /></label>
            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" MaxLength="100" />
            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPassword" ErrorMessage="<%$ Resources:SiteText, Register_Val_Password %>" Display="Dynamic" CssClass="field-error" />
            <asp:RegularExpressionValidator runat="server" ControlToValidate="txtPassword" ErrorMessage="<%$ Resources:SiteText, Register_Val_PasswordLength %>" ValidationExpression="^.{8,}$" Display="Dynamic" CssClass="field-error" />
        </div>
        <div class="form-row">
            <label for="<%= txtPasswordConfirm.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Register_Label_PasswordConfirm %>" /></label>
            <asp:TextBox ID="txtPasswordConfirm" runat="server" TextMode="Password" MaxLength="100" />
            <asp:CompareValidator runat="server" ControlToValidate="txtPasswordConfirm" ControlToCompare="txtPassword" ErrorMessage="<%$ Resources:SiteText, Register_Val_PasswordMismatch %>" Display="Dynamic" CssClass="field-error" />
        </div>
        <div class="form-row">
            <asp:Button ID="btnSubmit" runat="server" Text="<%$ Resources:SiteText, ResetPassword_Btn_Submit %>" OnClick="btnSubmit_Click" CssClass="btn-primary" />
        </div>
    </asp:Panel>
</asp:Content>
