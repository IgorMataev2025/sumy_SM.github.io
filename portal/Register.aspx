<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Register.aspx.vb" Inherits="SumyPortal.Register" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Реєстрація — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Literal runat="server" Text="<%$ Resources:SiteText, Register_Heading %>" /></h1>
    <p><asp:Literal runat="server" Text="<%$ Resources:SiteText, Register_Intro %>" /></p>

    <div class="account-type-choice">
        <asp:Panel runat="server" CssClass="account-type-card">
            <h2><asp:Literal runat="server" Text="<%$ Resources:SiteText, Register_Consumer_Title %>" /></h2>
            <p><asp:Literal runat="server" Text="<%$ Resources:SiteText, Register_Consumer_Desc %>" /></p>
            <asp:HyperLink runat="server" NavigateUrl="~/RegisterConsumer.aspx" CssClass="btn-primary" Text="<%$ Resources:SiteText, Register_Consumer_Btn %>" />
        </asp:Panel>

        <asp:Panel runat="server" CssClass="account-type-card">
            <h2><asp:Literal runat="server" Text="<%$ Resources:SiteText, Register_Provider_Title %>" /></h2>
            <p><asp:Literal runat="server" Text="<%$ Resources:SiteText, Register_Provider_Desc %>" /></p>
            <asp:HyperLink runat="server" NavigateUrl="~/RegisterProvider.aspx" CssClass="btn-primary" Text="<%$ Resources:SiteText, Register_Provider_Btn %>" />
        </asp:Panel>
    </div>
</asp:Content>
