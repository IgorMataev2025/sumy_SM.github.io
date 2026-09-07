<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="ForgotPassword.aspx.vb" Inherits="SumyPortal.ForgotPassword" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Відновлення пароля — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Відновлення пароля</h1>

    <asp:Panel ID="resultPanel" runat="server" Visible="false" CssClass="stub-note">
        <p>
            Якщо такий email зареєстрований, для нього створено посилання для
            скидання пароля.
        </p>
        <p>
            <b>Dev-режим</b> (реальна пошта не підключена): посилання нижче,
            якщо email існує в системі.
        </p>
        <asp:HyperLink ID="resetLink" runat="server" />
    </asp:Panel>

    <asp:Panel ID="formPanel" runat="server" CssClass="auth-form">
        <div class="form-row">
            <label for="<%= txtEmail.ClientID %>">Email</label>
            <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" MaxLength="255" />
            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmail" ErrorMessage="Вкажіть email" Display="Dynamic" CssClass="field-error" />
        </div>
        <div class="form-row">
            <asp:Button ID="btnSubmit" runat="server" Text="Надіслати посилання" OnClick="btnSubmit_Click" CssClass="btn-primary" />
        </div>
    </asp:Panel>
</asp:Content>
