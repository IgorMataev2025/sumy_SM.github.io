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

        <asp:HiddenField ID="hidLastMessageId" runat="server" />

        <div id="messagesContainer">
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
        </div>

        <asp:Panel ID="noMessagesPanel" runat="server" Visible="false" CssClass="stub-note">
            <asp:Literal runat="server" Text="<%$ Resources:SiteText, MessageThread_NoMessages %>" />
        </asp:Panel>

        <!-- "Жива" переписка (наступна фіча понад MVP, обрано автономно циклом /loop,
             2026-09-13) — простий AJAX-polling (MessagesPoll.ashx) кожні 5с, без
             WebSocket/SignalR (shared-хостинг навряд чи довгостроково підтримає постійні
             з'єднання, той самий принцип, що "poor man's cron" у Global.asax, п.20/п.25).
             Надсилання лишається звичайним постбеком (без змін) — живе лише отримання
             нових повідомлень від співрозмовника без перезавантаження сторінки. -->
        <script>
            (function () {
                var serviceId = <%= Request.QueryString("serviceId") %>;
                var consumerId = <%= Request.QueryString("consumerId") %>;
                var currentUserId = <%= CurrentUserId %>;
                var hidField = document.getElementById('<%= hidLastMessageId.ClientID %>');
                var container = document.getElementById('messagesContainer');
                var noMessagesPanel = document.getElementById('<%= noMessagesPanel.ClientID %>');
                var pollCount = 0;
                var maxPolls = 360; // ~30 хв при інтервалі 5с — не тримати з'єднання весь день відкритої вкладки

                function escapeHtml(text) {
                    var div = document.createElement('div');
                    div.textContent = text;
                    return div.innerHTML;
                }

                function poll() {
                    pollCount++;
                    if (pollCount > maxPolls) { clearInterval(timerId); return; }
                    if (document.hidden) return;

                    fetch('MessagesPoll.ashx?serviceId=' + serviceId + '&consumerId=' + consumerId + '&afterId=' + (hidField.value || '0'))
                        .then(function (r) { return r.json(); })
                        .then(function (messages) {
                            if (messages.length === 0) return;
                            if (noMessagesPanel) noMessagesPanel.style.display = 'none';

                            messages.forEach(function (m) {
                                var bubble = document.createElement('div');
                                bubble.className = 'service-card ' + (m.senderId === currentUserId ? 'status-approved' : 'status-draft');
                                bubble.innerHTML = '<p class="service-category"><b>' + escapeHtml(m.senderName) + '</b> · ' + escapeHtml(m.sentAt) + '</p>' +
                                    '<p class="message-body">' + escapeHtml(m.body) + '</p>';
                                container.appendChild(bubble);
                                hidField.value = m.messageId;
                            });
                        })
                        .catch(function () { /* тихо ігноруємо мережеву помилку — наступний polling спробує знову */ });
                }

                var timerId = setInterval(poll, 5000);
            })();
        </script>

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
