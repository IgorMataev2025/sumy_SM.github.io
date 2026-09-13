Imports System
Imports System.Text
Imports System.Web

Namespace SumyPortal

    ''' <summary>
    ''' Обробник сигналізації WebRTC-відеодзвінків (наступна фіча понад MVP, обрано
    ''' користувачем, 2026-09-13) — GET опитує нові сигнали (offer/answer/ICE/hangup),
    ''' POST надсилає власний. Той самий прийом, що MessagesPoll.ashx (App_Code, не
    ''' окремий CodeFile — стандартно для .ashx у файловій моделі Website Project),
    ''' і той самий захист доступу (лише двом учасникам розмови MessageThread.aspx) —
    ''' сервер тут лише передає непрозорі SDP/ICE-рядки, саму медіа-доріжку не бачить.
    ''' </summary>
    Public Class CallSignal
        Implements IHttpHandler

        Public ReadOnly Property IsReusable As Boolean Implements IHttpHandler.IsReusable
            Get
                Return True
            End Get
        End Property

        Public Sub ProcessRequest(context As HttpContext) Implements IHttpHandler.ProcessRequest
            context.Response.ContentType = "application/json; charset=utf-8"

            Dim currentUser = UserAccount.FindByEmail(context.User.Identity.Name)
            If currentUser Is Nothing Then
                context.Response.StatusCode = 403
                context.Response.Write("[]")
                Return
            End If

            ' POST (надсилання сигналу) читає serviceId/consumerId з тіла форми,
            ' GET (polling) — з рядка запиту (той самий поділ, що MessagesPoll.ashx,
            ' де сигналізація лише опитує — тут іще й надсилає).
            Dim isPost = (context.Request.HttpMethod = "POST")
            Dim serviceId As Integer, consumerId As Integer
            Dim parsedOk As Boolean
            If isPost Then
                parsedOk = Integer.TryParse(context.Request.Form("serviceId"), serviceId) AndAlso
                           Integer.TryParse(context.Request.Form("consumerId"), consumerId)
            Else
                parsedOk = Integer.TryParse(context.Request.QueryString("serviceId"), serviceId) AndAlso
                           Integer.TryParse(context.Request.QueryString("consumerId"), consumerId)
            End If
            If Not parsedOk Then
                context.Response.StatusCode = 400
                context.Response.Write("[]")
                Return
            End If

            Dim svc = Service.GetForMessaging(serviceId)
            If svc Is Nothing Then
                context.Response.Write("[]")
                Return
            End If

            ' Той самий захист доступу, що MessagesPoll.ashx/MessageThread.aspx.vb.
            Dim isParticipant = (currentUser.UserId = consumerId) OrElse (currentUser.UserId = svc.ProviderId)
            If Not isParticipant Then
                context.Response.StatusCode = 403
                context.Response.Write("[]")
                Return
            End If

            If isPost Then
                SendSignal(context, serviceId, consumerId, currentUser.UserId)
            Else
                PollSignals(context, serviceId, consumerId, currentUser.UserId)
            End If
        End Sub

        Private Sub SendSignal(context As HttpContext, serviceId As Integer, consumerId As Integer, currentUserId As Integer)
            Dim signalType = context.Request.Form("signalType")
            Dim payload = context.Request.Form("payload")

            ' Whitelist фіксованих значень з клієнтського JS — той самий принцип, що
            ' Service.BuildSortOrder (захист від довільного рядка в ENUM-стовпці).
            Dim allowedTypes = {"offer", "answer", "ice-candidate", "hangup"}
            If Not Array.Exists(allowedTypes, Function(t) t = signalType) Then
                context.Response.StatusCode = 400
                context.Response.Write("{""ok"":false}")
                Return
            End If

            ' Payload — SDP-блок для offer/answer може бути довгим (кілька КБ з кодеками);
            ' MySQL TEXT (до 64КБ) з великим запасом. hangup не потребує даних.
            If payload Is Nothing Then payload = ""
            If payload.Length > 60000 Then payload = payload.Substring(0, 60000)

            VideoCallSignal.Send(serviceId, consumerId, currentUserId, signalType, payload)
            context.Response.Write("{""ok"":true}")
        End Sub

        Private Sub PollSignals(context As HttpContext, serviceId As Integer, consumerId As Integer, currentUserId As Integer)
            Dim afterId As Integer
            Integer.TryParse(context.Request.QueryString("afterId"), afterId)

            Dim signals = VideoCallSignal.GetSignalsAfter(serviceId, consumerId, currentUserId, afterId)

            Dim sb As New StringBuilder("[")
            For i = 0 To signals.Count - 1
                If i > 0 Then sb.Append(","c)
                Dim s = signals(i)
                sb.Append("{""signalId"":").Append(s.SignalId)
                sb.Append(",""senderId"":").Append(s.SenderId)
                sb.Append(",""signalType"":""").Append(s.SignalType).Append("""")
                sb.Append(",""payload"":""").Append(JsonEscape(s.Payload)).Append("""}")
            Next
            sb.Append("]"c)

            context.Response.Write(sb.ToString())
        End Sub

        ''' <summary>Той самий мінімальний JSON-екранувач, що MessagesPoll.ashx.</summary>
        Private Function JsonEscape(text As String) As String
            Return text.Replace("\", "\\").Replace("""", "\""").Replace(vbCr, "").Replace(vbLf, "\n")
        End Function

    End Class

End Namespace
