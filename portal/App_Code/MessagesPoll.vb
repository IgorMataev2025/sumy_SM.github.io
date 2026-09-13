Imports System
Imports System.Text
Imports System.Web

Namespace SumyPortal

    ''' <summary>
    ''' Легкий JSON-обробник для "живої" переписки на MessageThread.aspx (наступна
    ''' фіча понад MVP, обрано автономно циклом /loop, 2026-09-13) — клієнтський
    ''' JS періодично питає лише нові повідомлення (afterId) замість перезавантаження
    ''' сторінки. Клас лежить в App_Code (не окремий CodeBehind) — той самий прийом,
    ''' що вже SetLanguage.ashx/Sitemap.ashx, стандартний для .ashx у файловій моделі
    ''' Website Project (README.md).
    '''
    ''' На відміну від SetLanguage.ashx (свій <location> з allow users="*") цей
    ''' обробник НЕ має винятку в Web.config — лишається за звичайним
    ''' `deny users="?"`, бо переписка й так вимагає входу; додатково перевіряє,
    ''' що поточний користувач — один із двох учасників саме цієї розмови (той
    ''' самий захист, що MessageThread.aspx.vb).
    ''' </summary>
    Public Class MessagesPoll
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

            Dim serviceId As Integer, consumerId As Integer, afterId As Integer
            If Not Integer.TryParse(context.Request.QueryString("serviceId"), serviceId) OrElse
               Not Integer.TryParse(context.Request.QueryString("consumerId"), consumerId) OrElse
               Not Integer.TryParse(context.Request.QueryString("afterId"), afterId) Then
                context.Response.StatusCode = 400
                context.Response.Write("[]")
                Return
            End If

            Dim svc = Service.GetForMessaging(serviceId)
            If svc Is Nothing Then
                context.Response.Write("[]")
                Return
            End If

            ' Той самий захист доступу, що MessageThread.aspx.vb — лише двом учасникам розмови.
            Dim isParticipant = (currentUser.UserId = consumerId) OrElse (currentUser.UserId = svc.ProviderId)
            If Not isParticipant Then
                context.Response.StatusCode = 403
                context.Response.Write("[]")
                Return
            End If

            Dim newMessages = DialogMessage.GetThreadAfter(serviceId, consumerId, afterId)

            Dim sb As New StringBuilder("[")
            For i = 0 To newMessages.Count - 1
                If i > 0 Then sb.Append(","c)
                Dim m = newMessages(i)
                sb.Append("{""messageId"":").Append(m.MessageId)
                sb.Append(",""senderId"":").Append(m.SenderId)
                sb.Append(",""senderName"":""").Append(JsonEscape(m.SenderName)).Append("""")
                sb.Append(",""body"":""").Append(JsonEscape(m.Body)).Append(""",")
                sb.Append("""sentAt"":""").Append(m.SentAt.ToString("dd.MM.yyyy HH:mm")).Append("""}")
            Next
            sb.Append("]"c)

            context.Response.Write(sb.ToString())
        End Sub

        ''' <summary>Мінімальне екранування для рядкового значення в JSON (лише спецсимволи,
        ''' достатні для валідного JSON) — HTML-екранування самого тексту при вставці в DOM
        ''' відбувається окремо на клієнті (MessageThread.aspx, escapeHtml), не тут.</summary>
        Private Function JsonEscape(text As String) As String
            Return text.Replace("\", "\\").Replace("""", "\""").Replace(vbCr, "").Replace(vbLf, "\n")
        End Function

    End Class

End Namespace
