<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="MessageThread.aspx.vb" Inherits="SumyPortal.MessageThread" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Розмова — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <p><a href="~/Messages.aspx" runat="server">← Усі повідомлення</a></p>

    <asp:Panel ID="notFoundPanel" runat="server" Visible="false" CssClass="stub-note">
        Розмову не знайдено, або у вас немає до неї доступу.
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
                    <p><%#: CType(Container.DataItem, SumyPortal.DialogMessage).Body %></p>
                </div>
            </ItemTemplate>
        </asp:Repeater>

        <asp:Panel ID="noMessagesPanel" runat="server" Visible="false" CssClass="stub-note">
            Повідомлень ще немає — напишіть перше.
        </asp:Panel>

        <asp:Panel ID="replyFormPanel" runat="server" CssClass="auth-form">
            <asp:Label ID="errorLabel" runat="server" CssClass="form-error" Visible="false" />
            <div class="form-row">
                <label for="<%= txtBody.ClientID %>">Повідомлення</label>
                <asp:TextBox ID="txtBody" runat="server" TextMode="MultiLine" Rows="3" MaxLength="2000" />
            </div>
            <div class="form-row">
                <asp:Button ID="btnSend" runat="server" Text="Надіслати" OnClick="btnSend_Click" CssClass="btn-primary" />
            </div>
        </asp:Panel>
    </asp:Panel>
</asp:Content>
