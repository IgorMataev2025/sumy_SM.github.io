<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="DonationResult.aspx.vb" Inherits="SumyPortal.DonationResult" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Підтримка проєкту — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Підтримка проєкту</h1>

    <asp:Panel ID="successPanel" runat="server" Visible="false" CssClass="stub-note">
        <p>Дякуємо! Ваш внесок (<asp:Literal ID="successAmountLiteral" runat="server" />) отримано.</p>
    </asp:Panel>

    <asp:Panel ID="pendingPanel" runat="server" Visible="false" CssClass="stub-note">
        <p>
            Дякуємо! Оплата обробляється — статус оновиться протягом кількох
            хвилин після підтвердження від LiqPay.
        </p>
    </asp:Panel>

    <asp:Panel ID="failurePanel" runat="server" Visible="false" CssClass="stub-note">
        <p>На жаль, оплату не вдалося завершити.</p>
        <p><asp:HyperLink runat="server" NavigateUrl="~/Donate.aspx">← Спробувати ще раз</asp:HyperLink></p>
    </asp:Panel>

    <asp:Panel ID="notFoundPanel" runat="server" Visible="false" CssClass="stub-note">
        <p>Внесок не знайдено.</p>
    </asp:Panel>

    <p><asp:HyperLink runat="server" NavigateUrl="~/Default.aspx">← На головну</asp:HyperLink></p>
</asp:Content>
