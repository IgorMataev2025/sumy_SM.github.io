<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Login.aspx.vb" Inherits="SumyPortal.Login" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Вхід — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Literal runat="server" Text="<%$ Resources:SiteText, Login_Heading %>" /></h1>

    <asp:Panel ID="formPanel" runat="server" CssClass="auth-form">
        <asp:Label ID="serverErrorLabel" runat="server" CssClass="form-error" Visible="false" />

        <div class="form-row">
            <label for="<%= txtEmail.ClientID %>">Email</label>
            <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" MaxLength="255" />
            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmail" ErrorMessage="<%$ Resources:SiteText, Login_Val_Email %>" Display="Dynamic" CssClass="field-error" />
        </div>

        <div class="form-row">
            <label for="<%= txtPassword.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Login_Label_Password %>" /></label>
            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" MaxLength="100" />
            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPassword" ErrorMessage="<%$ Resources:SiteText, Login_Val_Password %>" Display="Dynamic" CssClass="field-error" />
        </div>

        <div class="form-row">
            <asp:Button ID="btnLogin" runat="server" Text="<%$ Resources:SiteText, Login_Btn_Submit %>" OnClick="btnLogin_Click" CssClass="btn-primary" />
        </div>

        <p><asp:HyperLink ID="forgotPasswordLink" runat="server" NavigateUrl="~/ForgotPassword.aspx" Text="<%$ Resources:SiteText, Login_ForgotPassword %>" /></p>
        <p><asp:Literal runat="server" Text="<%$ Resources:SiteText, Login_NoAccount_Prefix %>" /> <asp:HyperLink runat="server" NavigateUrl="~/Register.aspx" Text="<%$ Resources:SiteText, Login_Register_Link %>" /></p>
    </asp:Panel>
</asp:Content>
