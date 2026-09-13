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

        <!-- Відеодзвінок (наступна фіча понад MVP, обрано користувачем, 2026-09-13) —
             WebRTC P2P (браузер↔браузер, лише сигналізація через CallSignal.ashx,
             сама аудіо/відео-доріжка сюди не потрапляє). ВАЖЛИВО: надсилання звичайного
             текстового повідомлення нижче — постбек з повним перезавантаженням
             сторінки (Response.Redirect), що обірве активний дзвінок — свідоме
             MVP-обмеження, не вирішується в цій фічі (переведення всієї відправки на
             AJAX — окрема більша задача). TURN-сервер не підключено (лише безкоштовний
             публічний STUN) — з'єднання може не встановитись за складним NAT/файрволом. -->
        <div class="video-call-section">
            <button type="button" id="btnStartCall" class="btn-secondary"><asp:Literal runat="server" Text="<%$ Resources:SiteText, MessageThread_Call_Start %>" /></button>

            <div id="incomingCallBanner" class="stub-note" style="display:none;">
                <asp:Literal runat="server" Text="<%$ Resources:SiteText, MessageThread_Call_Incoming %>" />
                <button type="button" id="btnAcceptCall" class="btn-primary"><asp:Literal runat="server" Text="<%$ Resources:SiteText, MessageThread_Call_Accept %>" /></button>
                <button type="button" id="btnRejectCall" class="btn-secondary"><asp:Literal runat="server" Text="<%$ Resources:SiteText, MessageThread_Call_Reject %>" /></button>
            </div>

            <div id="videoCallPanel" style="display:none;">
                <p id="callStatusText" class="stub-note"></p>
                <div class="video-grid">
                    <video id="localVideo" autoplay playsinline muted></video>
                    <video id="remoteVideo" autoplay playsinline></video>
                </div>
                <button type="button" id="btnHangup" class="btn-secondary"><asp:Literal runat="server" Text="<%$ Resources:SiteText, MessageThread_Call_Hangup %>" /></button>
            </div>
        </div>

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

        <!-- WebRTC відеодзвінок (наступна фіча понад MVP, обрано користувачем, 2026-09-13).
             Сигналізація (offer/answer/ICE) — AJAX-polling кожні 3с (CallSignal.ashx),
             той самий принцип без WebSocket, що вже жива переписка вище. Лише публічний
             STUN (Google) — без TURN, тому з'єднання може не встановитись за складним
             NAT/корпоративним фаєрволом (свідоме обмеження MVP, немає платної
             інфраструктури під TURN). -->
        <script>
            (function () {
                var serviceId = <%= Request.QueryString("serviceId") %>;
                var consumerId = <%= Request.QueryString("consumerId") %>;

                var iceServers = [
                    { urls: 'stun:stun.l.google.com:19302' },
                    { urls: 'stun:stun1.l.google.com:19302' }
                ];

                var btnStartCall = document.getElementById('btnStartCall');
                var incomingCallBanner = document.getElementById('incomingCallBanner');
                var btnAcceptCall = document.getElementById('btnAcceptCall');
                var btnRejectCall = document.getElementById('btnRejectCall');
                var videoCallPanel = document.getElementById('videoCallPanel');
                var callStatusText = document.getElementById('callStatusText');
                var localVideo = document.getElementById('localVideo');
                var remoteVideo = document.getElementById('remoteVideo');
                var btnHangup = document.getElementById('btnHangup');

                var pc = null;
                var localStream = null;
                var pendingOffer = null;
                var pendingIce = [];
                var lastSignalId = 0;

                function setStatus(text) { callStatusText.textContent = text; }
                function showCallPanel(visible) { videoCallPanel.style.display = visible ? 'block' : 'none'; btnStartCall.style.display = visible ? 'none' : 'inline-block'; }
                function showIncomingBanner(visible) { incomingCallBanner.style.display = visible ? 'block' : 'none'; }

                function sendSignal(signalType, payload) {
                    var body = new URLSearchParams();
                    body.append('serviceId', serviceId);
                    body.append('consumerId', consumerId);
                    body.append('signalType', signalType);
                    body.append('payload', payload || '');
                    fetch('CallSignal.ashx', { method: 'POST', body: body }).catch(function () { });
                }

                function createPeerConnection() {
                    var conn = new RTCPeerConnection({ iceServers: iceServers });
                    conn.onicecandidate = function (e) {
                        if (e.candidate) sendSignal('ice-candidate', JSON.stringify(e.candidate));
                    };
                    conn.ontrack = function (e) {
                        remoteVideo.srcObject = e.streams[0];
                        setStatus('<%= Resources.SiteText.MessageThread_Call_Status_Connected %>');
                    };
                    conn.onconnectionstatechange = function () {
                        if (conn.connectionState === 'failed' || conn.connectionState === 'disconnected') {
                            endCall(false);
                        }
                    };
                    return conn;
                }

                function flushPendingIce() {
                    pendingIce.forEach(function (c) { pc.addIceCandidate(new RTCIceCandidate(c)).catch(function () { }); });
                    pendingIce = [];
                }

                function startCall() {
                    navigator.mediaDevices.getUserMedia({ video: true, audio: true }).then(function (stream) {
                        localStream = stream;
                        localVideo.srcObject = stream;
                        pc = createPeerConnection();
                        stream.getTracks().forEach(function (t) { pc.addTrack(t, stream); });
                        showCallPanel(true);
                        setStatus('<%= Resources.SiteText.MessageThread_Call_Status_Calling %>');
                        return pc.createOffer();
                    }).then(function (offer) {
                        return pc.setLocalDescription(offer).then(function () { return offer; });
                    }).then(function (offer) {
                        sendSignal('offer', JSON.stringify(offer));
                    }).catch(function (err) {
                        alert('<%= Resources.SiteText.MessageThread_Call_Err_Media %>' + (err && err.message ? ' (' + err.message + ')' : ''));
                    });
                }

                function acceptCall() {
                    showIncomingBanner(false);
                    navigator.mediaDevices.getUserMedia({ video: true, audio: true }).then(function (stream) {
                        localStream = stream;
                        localVideo.srcObject = stream;
                        pc = createPeerConnection();
                        stream.getTracks().forEach(function (t) { pc.addTrack(t, stream); });
                        return pc.setRemoteDescription(new RTCSessionDescription(pendingOffer));
                    }).then(function () {
                        showCallPanel(true);
                        setStatus('<%= Resources.SiteText.MessageThread_Call_Status_Connecting %>');
                        return pc.createAnswer();
                    }).then(function (answer) {
                        return pc.setLocalDescription(answer).then(function () { return answer; });
                    }).then(function (answer) {
                        sendSignal('answer', JSON.stringify(answer));
                        flushPendingIce();
                    }).catch(function (err) {
                        alert('<%= Resources.SiteText.MessageThread_Call_Err_Media %>' + (err && err.message ? ' (' + err.message + ')' : ''));
                        sendSignal('hangup', '');
                    });
                }

                function rejectCall() {
                    showIncomingBanner(false);
                    pendingOffer = null;
                    sendSignal('hangup', '');
                }

                function endCall(notifyOther) {
                    if (notifyOther) sendSignal('hangup', '');
                    if (pc) { pc.close(); pc = null; }
                    if (localStream) { localStream.getTracks().forEach(function (t) { t.stop(); }); localStream = null; }
                    localVideo.srcObject = null;
                    remoteVideo.srcObject = null;
                    showCallPanel(false);
                    showIncomingBanner(false);
                    pendingOffer = null;
                    pendingIce = [];
                }

                function handleIncomingOffer(signal) {
                    if (pc) return; // вже в дзвінку — ігноруємо повторний offer
                    pendingOffer = JSON.parse(signal.payload);
                    showIncomingBanner(true);
                }

                function handleAnswer(signal) {
                    if (!pc) return;
                    pc.setRemoteDescription(new RTCSessionDescription(JSON.parse(signal.payload))).then(function () {
                        setStatus('<%= Resources.SiteText.MessageThread_Call_Status_Connecting %>');
                        flushPendingIce();
                    }).catch(function () { });
                }

                function handleIceCandidate(signal) {
                    var candidate = JSON.parse(signal.payload);
                    if (pc && pc.remoteDescription && pc.remoteDescription.type) {
                        pc.addIceCandidate(new RTCIceCandidate(candidate)).catch(function () { });
                    } else {
                        pendingIce.push(candidate);
                    }
                }

                function pollSignals() {
                    if (document.hidden) return;
                    fetch('CallSignal.ashx?serviceId=' + serviceId + '&consumerId=' + consumerId + '&afterId=' + lastSignalId)
                        .then(function (r) { return r.json(); })
                        .then(function (signals) {
                            signals.forEach(function (s) {
                                lastSignalId = s.signalId;
                                if (s.signalType === 'offer') handleIncomingOffer(s);
                                else if (s.signalType === 'answer') handleAnswer(s);
                                else if (s.signalType === 'ice-candidate') handleIceCandidate(s);
                                else if (s.signalType === 'hangup') endCall(false);
                            });
                        })
                        .catch(function () { });
                }

                btnStartCall.addEventListener('click', startCall);
                btnAcceptCall.addEventListener('click', acceptCall);
                btnRejectCall.addEventListener('click', rejectCall);
                btnHangup.addEventListener('click', function () { endCall(true); });

                if (typeof RTCPeerConnection === 'undefined' || !navigator.mediaDevices) {
                    btnStartCall.disabled = true;
                    btnStartCall.title = '<%= Resources.SiteText.MessageThread_Call_Err_Unsupported %>';
                } else {
                    setInterval(pollSignals, 3000);
                }
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
