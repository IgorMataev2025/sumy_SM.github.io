<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="MessageThread.aspx.vb" Inherits="SumyPortal.MessageThread" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Розмова — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <p><a href="~/Messages.aspx" runat="server"><asp:Literal runat="server" Text="<%$ Resources:SiteText, MessageThread_BackToAll %>" /></a></p>

    <asp:Panel ID="notFoundPanel" runat="server" Visible="false" CssClass="stub-note">
        <asp:Literal runat="server" Text="<%$ Resources:SiteText, MessageThread_NotFound %>" />
    </asp:Panel>

    <asp:Panel ID="threadPanel" runat="server">
        <h1><asp:Literal ID="headingLiteral" runat="server" /></h1>

        <asp:Repeater ID="rptMessages" runat="server">
            <ItemTemplate>
                <div class='<%#: "service-card " & If(CType(Container.DataItem, SumyPortal.DialogMessage).SenderId = CurrentUserId, "status-approved", "status-draft") %>'>
                    <p class="service-category">
                        <b><%#: CType(Container.DataItem, SumyPortal.DialogMessage).SenderName %></b> ·
                        <%#: CType(Container.DataItem, SumyPortal.DialogMessage).SentAt.ToString("dd.MM.yyyy HH:mm") %>
                    </p>
                    <p class="message-body"><%#: CType(Container.DataItem, SumyPortal.DialogMessage).Body %></p>
                </div>
            </ItemTemplate>
        </asp:Repeater>

        <asp:Panel ID="noMessagesPanel" runat="server" Visible="false" CssClass="stub-note">
            <asp:Literal runat="server" Text="<%$ Resources:SiteText, MessageThread_NoMessages %>" />
        </asp:Panel>

        <asp:Panel ID="replyFormPanel" runat="server" CssClass="auth-form">
            <asp:Label ID="errorLabel" runat="server" CssClass="form-error" Visible="false" />

            <!-- Структурована заявка (постановка робочої тестової версії, 2026-09-12) —
                 лише для першого звернення споживача (порожня розмова): дата/адреса
                 об'єднуються в текст першого повідомлення при відправці, окремої
                 сутності/статусу заявки немає (рішення користувача — платформа про
                 стосунки, а не е-комерс із трекінгом виконання замовлення). -->
            <asp:Panel ID="orderFieldsPanel" runat="server" Visible="false">
                <div class="form-row">
                    <label for="<%= txtDesiredDate.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:SiteText, MessageThread_OrderDate_Label %>" /></label>
                    <asp:TextBox ID="txtDesiredDate" runat="server" MaxLength="100" />
                </div>
                <div class="form-row">
                    <label for="<%= txtAddress.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:SiteText, MessageThread_OrderAddress_Label %>" /></label>
                    <asp:TextBox ID="txtAddress" runat="server" MaxLength="255" />
                </div>
            </asp:Panel>

            <div class="form-row">
                <label for="<%= txtBody.ClientID %>"><asp:Literal ID="bodyLabelLiteral" runat="server" /></label>
                <asp:TextBox ID="txtBody" runat="server" TextMode="MultiLine" Rows="3" MaxLength="2000" />
            </div>
            <div class="form-row">
                <asp:Button ID="btnSend" runat="server" Text="<%$ Resources:SiteText, MessageThread_Btn_Send %>" OnClick="btnSend_Click" CssClass="btn-primary" />
            </div>
        </asp:Panel>
    </asp:Panel>
</asp:Content>
