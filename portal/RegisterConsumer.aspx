<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="RegisterConsumer.aspx.vb" Inherits="SumyPortal.RegisterConsumer" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Реєстрація споживача — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Literal runat="server" Text="<%$ Resources:SiteText, RegisterConsumer_Heading %>" /></h1>
    <p class="stub-note"><asp:HyperLink runat="server" NavigateUrl="~/Register.aspx" Text="<%$ Resources:SiteText, Register_BackChooseType %>" /></p>

    <asp:Panel ID="successPanel" runat="server" Visible="false" CssClass="stub-note">
        <p><asp:Literal runat="server" Text="<%$ Resources:SiteText, Register_Success_Thanks %>" /></p>
        <p>
            <b><asp:Literal runat="server" Text="<%$ Resources:SiteText, Register_DevMode_Label %>" /></b>
            <asp:Literal runat="server" Text="<%$ Resources:SiteText, Register_DevMode_Note %>" />
        </p>
        <p><asp:HyperLink ID="confirmLink" runat="server" Text="<%$ Resources:SiteText, Register_ConfirmEmailLink %>" /></p>
    </asp:Panel>

    <asp:Panel ID="formPanel" runat="server" CssClass="auth-form">
        <asp:ValidationSummary ID="validationSummary" runat="server" CssClass="form-error" DisplayMode="BulletList" />
        <asp:Label ID="serverErrorLabel" runat="server" CssClass="form-error" Visible="false" />

        <div class="form-row">
            <label for="<%= txtEmail.ClientID %>">Email</label>
            <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" MaxLength="255" />
            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmail" ErrorMessage="<%$ Resources:SiteText, Register_Val_Email %>" Display="Dynamic" CssClass="field-error" />
            <asp:RegularExpressionValidator runat="server" ControlToValidate="txtEmail" ErrorMessage="<%$ Resources:SiteText, Register_Val_EmailInvalid %>" ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$" Display="Dynamic" CssClass="field-error" />
        </div>

        <div class="form-row">
            <label for="<%= txtPassword.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Register_Label_Password %>" /></label>
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
            <label for="<%= txtFullName.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Register_Label_FullName %>" /></label>
            <asp:TextBox ID="txtFullName" runat="server" MaxLength="255" />
            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtFullName" ErrorMessage="<%$ Resources:SiteText, Register_Val_FullName %>" Display="Dynamic" CssClass="field-error" />
        </div>

        <div class="form-row">
            <label for="<%= txtPhone.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Register_Label_Phone %>" /></label>
            <asp:TextBox ID="txtPhone" runat="server" MaxLength="50" />
        </div>

        <div class="form-row">
            <label for="<%= ddlDistrict.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Register_Label_District %>" /></label>
            <asp:DropDownList ID="ddlDistrict" runat="server" />
        </div>

        <div class="form-row">
            <asp:CheckBox ID="chkPrivacyConsent" runat="server" />
            <label for="<%= chkPrivacyConsent.ClientID %>">
                <asp:Literal runat="server" Text="<%$ Resources:SiteText, Register_Consent_Prefix %>" /> <asp:HyperLink runat="server" NavigateUrl="~/PrivacyPolicy.aspx" Target="_blank" Text="<%$ Resources:SiteText, Register_Consent_PrivacyLink %>" />
                <asp:Literal runat="server" Text="<%$ Resources:SiteText, Register_Consent_Suffix %>" />
            </label>
        </div>

        <div class="form-row">
            <asp:Button ID="btnRegister" runat="server" Text="<%$ Resources:SiteText, Register_Btn_Submit %>" OnClick="btnRegister_Click" CssClass="btn-primary" />
        </div>
    </asp:Panel>
</asp:Content>
