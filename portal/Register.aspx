<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Register.aspx.vb" Inherits="SumyPortal.Register" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Реєстрація — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Реєстрація</h1>
    <p>Постачальник і Споживач реєструються окремо — оберіть свій тип акаунта:</p>

    <div class="account-type-choice">
        <asp:Panel runat="server" CssClass="account-type-card">
            <h2>Споживач</h2>
            <p>Шукаю послуги</p>
            <asp:HyperLink runat="server" NavigateUrl="~/RegisterConsumer.aspx" CssClass="btn-primary">Зареєструватися як споживач</asp:HyperLink>
        </asp:Panel>

        <asp:Panel runat="server" CssClass="account-type-card">
            <h2>Постачальник</h2>
            <p>Надаю послуги</p>
            <asp:HyperLink runat="server" NavigateUrl="~/RegisterProvider.aspx" CssClass="btn-primary">Зареєструватися як постачальник</asp:HyperLink>
        </asp:Panel>
    </div>
</asp:Content>
