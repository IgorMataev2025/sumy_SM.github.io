<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="ResetPassword.aspx.vb" Inherits="SumyPortal.ResetPassword" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Новий пароль — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Встановлення нового пароля</h1>

    <asp:Panel ID="invalidTokenPanel" runat="server" Visible="false" CssClass="stub-note">
        Посилання недійсне або вже використане. Спробуйте
        <a href="ForgotPassword.aspx">запросити нове</a>.
    </asp:Panel>

    <asp:Panel ID="successPanel" runat="server" Visible="false" CssClass="stub-note">
        Пароль оновлено. Тепер можна <a href="Login.aspx">увійти</a>.
    </asp:Panel>

    <asp:Panel ID="formPanel" runat="server" CssClass="auth-form">
        <asp:Label ID="serverErrorLabel" runat="server" CssClass="form-error" Visible="false" />
        <div class="form-row">
            <label for="<%= txtPassword.ClientID %>">Новий пароль (мінімум 8 символів)</label>
            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" MaxLength="100" />
            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPassword" ErrorMessage="Вкажіть пароль" Display="Dynamic" CssClass="field-error" />
            <asp:RegularExpressionValidator runat="server" ControlToValidate="txtPassword" ErrorMessage="Пароль має бути не коротшим за 8 символів" ValidationExpression="^.{8,}$" Display="Dynamic" CssClass="field-error" />
        </div>
        <div class="form-row">
            <label for="<%= txtPasswordConfirm.ClientID %>">Повторіть пароль</label>
            <asp:TextBox ID="txtPasswordConfirm" runat="server" TextMode="Password" MaxLength="100" />
            <asp:CompareValidator runat="server" ControlToValidate="txtPasswordConfirm" ControlToCompare="txtPassword" ErrorMessage="Паролі не збігаються" Display="Dynamic" CssClass="field-error" />
        </div>
        <div class="form-row">
            <asp:Button ID="btnSubmit" runat="server" Text="Зберегти пароль" OnClick="btnSubmit_Click" CssClass="btn-primary" />
        </div>
    </asp:Panel>
</asp:Content>
