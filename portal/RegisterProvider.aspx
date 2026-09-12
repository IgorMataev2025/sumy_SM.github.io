<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="RegisterProvider.aspx.vb" Inherits="SumyPortal.RegisterProvider" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Реєстрація постачальника — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Реєстрація: Постачальник (надаю послуги)</h1>
    <p class="stub-note"><asp:HyperLink runat="server" NavigateUrl="~/Register.aspx">← Не той тип акаунта? Оберіть інший</asp:HyperLink></p>

    <asp:Panel ID="successPanel" runat="server" Visible="false" CssClass="stub-note">
        <p>Дякуємо за реєстрацію! Лишилось підтвердити email.</p>
        <p>
            <b>Dev-режим</b> (реальна пошта поки не підключена — див. README):
            посилання для підтвердження нижче, у бойовому середовищі воно піде листом.
        </p>
        <p><asp:HyperLink ID="confirmLink" runat="server">Підтвердити email</asp:HyperLink></p>
    </asp:Panel>

    <asp:Panel ID="formPanel" runat="server" CssClass="auth-form">
        <asp:ValidationSummary ID="validationSummary" runat="server" CssClass="form-error" DisplayMode="BulletList" />
        <asp:Label ID="serverErrorLabel" runat="server" CssClass="form-error" Visible="false" />

        <div class="form-row">
            <label for="<%= txtEmail.ClientID %>">Email</label>
            <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" MaxLength="255" />
            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmail" ErrorMessage="Вкажіть email" Display="Dynamic" CssClass="field-error" />
            <asp:RegularExpressionValidator runat="server" ControlToValidate="txtEmail" ErrorMessage="Некоректний email" ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$" Display="Dynamic" CssClass="field-error" />
        </div>

        <div class="form-row">
            <label for="<%= txtPassword.ClientID %>">Пароль (мінімум 8 символів)</label>
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

        <div class="form-row">
            <asp:CheckBox ID="chkLegalEntity" runat="server" Text="Юридична особа" AutoPostBack="true" CausesValidation="false" OnCheckedChanged="chkLegalEntity_CheckedChanged" />
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
            <asp:CheckBox ID="chkPrivacyConsent" runat="server" />
            <label for="<%= chkPrivacyConsent.ClientID %>">
                Я ознайомлений(-а) з <asp:HyperLink runat="server" NavigateUrl="~/PrivacyPolicy.aspx" Target="_blank">Політикою конфіденційності</asp:HyperLink>
                і надаю згоду на обробку персональних даних
            </label>
        </div>

        <div class="form-row">
            <asp:Button ID="btnRegister" runat="server" Text="Зареєструватися" OnClick="btnRegister_Click" CssClass="btn-primary" />
        </div>
    </asp:Panel>
</asp:Content>
