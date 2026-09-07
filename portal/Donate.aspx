<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Donate.aspx.vb" Inherits="SumyPortal.Donate" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Підтримати проєкт — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Підтримати проєкт Safina</h1>
    <p>
        Портал Safina розвивається як волонтерський проєкт для мешканців Сум та
        області. Якщо він виявився корисним, ви можете підтримати його розвиток
        добровільним внеском — оплата проходить через LiqPay, дані картки
        порталу не бачить і не зберігає.
    </p>

    <asp:Label ID="serverErrorLabel" runat="server" CssClass="form-error" Visible="false" />

    <asp:Panel ID="formPanel" runat="server" CssClass="auth-form">
        <asp:ValidationSummary ID="validationSummary" runat="server" CssClass="form-error" DisplayMode="BulletList" />

        <fieldset class="form-row">
            <legend>Сума внеску, грн</legend>
            <asp:RadioButtonList ID="rblAmount" runat="server" RepeatDirection="Horizontal">
                <asp:ListItem Text="50" Value="50" />
                <asp:ListItem Text="100" Value="100" Selected="True" />
                <asp:ListItem Text="300" Value="300" />
                <asp:ListItem Text="500" Value="500" />
                <asp:ListItem Text="Інша сума" Value="custom" />
            </asp:RadioButtonList>
        </fieldset>

        <div class="form-row">
            <label for="<%= txtCustomAmount.ClientID %>">Своя сума (грн, якщо обрано «Інша сума»)</label>
            <asp:TextBox ID="txtCustomAmount" runat="server" TextMode="Number" />
        </div>

        <div class="form-row">
            <label for="<%= txtDonorName.ClientID %>">Ім'я (необов'язково)</label>
            <asp:TextBox ID="txtDonorName" runat="server" MaxLength="255" />
        </div>

        <div class="form-row">
            <label for="<%= txtDonorEmail.ClientID %>">Email для квитанції (необов'язково)</label>
            <asp:TextBox ID="txtDonorEmail" runat="server" TextMode="Email" MaxLength="255" />
            <asp:RegularExpressionValidator runat="server" ControlToValidate="txtDonorEmail" ErrorMessage="Некоректний email" ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$" Display="Dynamic" CssClass="field-error" />
        </div>

        <div class="form-row">
            <asp:Button ID="btnDonate" runat="server" Text="Підтримати через LiqPay" OnClick="btnDonate_Click" CssClass="btn-primary" />
        </div>
    </asp:Panel>
</asp:Content>
