<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="AdminDashboard.aspx.vb" Inherits="SumyPortal.AdminDashboard" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Адмін-панель — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Адмін-панель</h1>
    <p class="stub-note">
        Ця сторінка не пов'язана з головним меню — збережіть її в закладках,
        щоб повертатись напряму.
    </p>

    <div class="categories">
        <a class="category-card" href="~/AdminModeration.aspx" runat="server">
            <h3>Модерація оголошень</h3>
            <p><asp:Literal ID="pendingCountLiteral" runat="server" /></p>
        </a>
        <a class="category-card" href="~/AdminServices.aspx" runat="server">
            <h3>Усі оголошення</h3>
            <p><asp:Literal ID="totalServicesCountLiteral" runat="server" /></p>
        </a>
        <a class="category-card" href="~/AdminCategories.aspx" runat="server">
            <h3>Категорії послуг</h3>
            <p><asp:Literal ID="categoryCountLiteral" runat="server" /></p>
        </a>
        <a class="category-card" href="~/AdminUsers.aspx" runat="server">
            <h3>Користувачі</h3>
            <p><asp:Literal ID="userCountLiteral" runat="server" /></p>
        </a>
        <a class="category-card" href="~/AdminLog.aspx" runat="server">
            <h3>Журнал дій</h3>
            <p><asp:Literal ID="logCountLiteral" runat="server" /></p>
        </a>
    </div>
</asp:Content>
