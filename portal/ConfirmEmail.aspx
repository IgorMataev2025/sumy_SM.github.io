<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="ConfirmEmail.aspx.vb" Inherits="SumyPortal.ConfirmEmail" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Підтвердження email — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Підтвердження email</h1>
    <asp:Panel ID="resultPanel" runat="server" CssClass="stub-note">
        <asp:Literal ID="resultText" runat="server" />
    </asp:Panel>
</asp:Content>
