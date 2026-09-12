<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="DonationResult.aspx.vb" Inherits="SumyPortal.DonationResult" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Підтримка проєкту — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Literal runat="server" Text="<%$ Resources:SiteText, DonationResult_Heading %>" /></h1>

    <asp:Panel ID="successPanel" runat="server" Visible="false" CssClass="stub-note">
        <p><asp:Literal ID="successTextLiteral" runat="server" /></p>
    </asp:Panel>

    <asp:Panel ID="pendingPanel" runat="server" Visible="false" CssClass="stub-note">
        <p>
            <asp:Literal runat="server" Text="<%$ Resources:SiteText, DonationResult_Pending %>" />
        </p>
    </asp:Panel>

    <asp:Panel ID="failurePanel" runat="server" Visible="false" CssClass="stub-note">
        <p><asp:Literal runat="server" Text="<%$ Resources:SiteText, DonationResult_Failure %>" /></p>
        <p><asp:HyperLink runat="server" NavigateUrl="~/Donate.aspx" Text="<%$ Resources:SiteText, DonationResult_RetryLink %>" /></p>
    </asp:Panel>

    <asp:Panel ID="notFoundPanel" runat="server" Visible="false" CssClass="stub-note">
        <p><asp:Literal runat="server" Text="<%$ Resources:SiteText, DonationResult_NotFound %>" /></p>
    </asp:Panel>

    <p><asp:HyperLink runat="server" NavigateUrl="~/Default.aspx" Text="<%$ Resources:SiteText, DonationResult_BackHome %>" /></p>
</asp:Content>
