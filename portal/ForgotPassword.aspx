<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="ForgotPassword.aspx.vb" Inherits="SumyPortal.ForgotPassword" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Відновлення пароля — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Literal runat="server" Text="<%$ Resources:SiteText, ForgotPassword_Heading %>" /></h1>

    <asp:Panel ID="resultPanel" runat="server" Visible="false" CssClass="stub-note">
        <p>
            <asp:Literal runat="server" Text="<%$ Resources:SiteText, ForgotPassword_Result_Text %>" />
        </p>
        <p>
            <b><asp:Literal runat="server" Text="<%$ Resources:SiteText, Register_DevMode_Label %>" /></b>
            <asp:Literal runat="server" Text="<%$ Resources:SiteText, ForgotPassword_DevMode_Note %>" />
        </p>
        <asp:HyperLink ID="resetLink" runat="server" />
    </asp:Panel>

    <asp:Panel ID="formPanel" runat="server" CssClass="auth-form">
        <div class="form-row">
            <label for="<%= txtEmail.ClientID %>">Email</label>
            <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" MaxLength="255" />
            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmail" ErrorMessage="<%$ Resources:SiteText, ForgotPassword_Val_Email %>" Display="Dynamic" CssClass="field-error" />
        </div>
        <div class="form-row">
            <asp:Button ID="btnSubmit" runat="server" Text="<%$ Resources:SiteText, ForgotPassword_Btn_Submit %>" OnClick="btnSubmit_Click" CssClass="btn-primary" />
        </div>
    </asp:Panel>
</asp:Content>
