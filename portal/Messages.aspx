<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Messages.aspx.vb" Inherits="SumyPortal.Messages" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Повідомлення — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Literal runat="server" Text="<%$ Resources:SiteText, Nav_Messages %>" /></h1>

    <asp:Panel ID="emptyPanel" runat="server" Visible="false" CssClass="stub-note">
        <asp:Literal runat="server" Text="<%$ Resources:SiteText, Messages_Empty %>" />
    </asp:Panel>

    <asp:Repeater ID="rptConversations" runat="server">
        <ItemTemplate>
            <a class="category-card" href='<%#: ResolveUrl("~/MessageThread.aspx?serviceId=" & CType(Container.DataItem, SumyPortal.ConversationSummary).ServiceId & "&consumerId=" & CType(Container.DataItem, SumyPortal.ConversationSummary).ConsumerId) %>'>
                <h3><%#: CType(Container.DataItem, SumyPortal.ConversationSummary).ServiceTitle %> — <%#: CType(Container.DataItem, SumyPortal.ConversationSummary).OtherPartyName %></h3>
                <p><%#: CType(Container.DataItem, SumyPortal.ConversationSummary).LastBody %></p>
                <p class="service-category"><%#: CType(Container.DataItem, SumyPortal.ConversationSummary).LastSentAt.ToString("dd.MM.yyyy HH:mm") %></p>
            </a>
        </ItemTemplate>
    </asp:Repeater>
</asp:Content>
