Imports System

Namespace SumyPortal

    ''' <summary>
    ''' Одна розмова (п.10 уточненої постановки) — доступна лише двом учасникам:
    ''' цьому конкретному споживачу (consumerId у URL) і постачальнику оголошення
    ''' (Service.ProviderId). Перший захід споживача на ще порожню розмову теж
    ''' валідний — GetThread просто поверне порожній список, це і є "написати
    ''' перше повідомлення" (кнопка на ServiceDetails.aspx веде саме сюди).
    ''' </summary>
    Public Class MessageThread
        Inherits System.Web.UI.Page

        ''' <summary>Для розмітки — щоб відрізнити "свої" бульбашки від "чужих" (CssClass у Repeater).</summary>
        Protected Property CurrentUserId As Integer

        Private _serviceId As Integer
        Private _consumerId As Integer
        Private _svc As Service

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            Dim currentUser = UserAccount.FindByEmail(Page.User.Identity.Name)
            If currentUser Is Nothing Then
                ShowNotFound()
                Return
            End If
            CurrentUserId = currentUser.UserId

            If Not Integer.TryParse(Request.QueryString("serviceId"), _serviceId) OrElse
               Not Integer.TryParse(Request.QueryString("consumerId"), _consumerId) Then
                ShowNotFound()
                Return
            End If

            _svc = Service.GetForMessaging(_serviceId)
            If _svc Is Nothing Then
                ShowNotFound()
                Return
            End If

            ' Доступ лише двом учасникам саме цієї розмови (не будь-якому Consumer/Provider).
            Dim isParticipant = (currentUser.UserId = _consumerId) OrElse (currentUser.UserId = _svc.ProviderId)
            If Not isParticipant Then
                ShowNotFound()
                Return
            End If

            headingLiteral.Text = _svc.Title

            If Not IsPostBack Then
                BindThread()
            End If
        End Sub

        Private Sub BindThread()
            Dim thread = DialogMessage.GetThread(_serviceId, _consumerId)
            rptMessages.DataSource = thread
            rptMessages.DataBind()
            noMessagesPanel.Visible = (thread.Count = 0)
        End Sub

        Protected Sub btnSend_Click(sender As Object, e As EventArgs)
            Dim body = txtBody.Text.Trim()
            If String.IsNullOrEmpty(body) Then
                errorLabel.Text = "Введіть текст повідомлення."
                errorLabel.Visible = True
                Return
            End If

            DialogMessage.Send(_serviceId, _consumerId, CurrentUserId, body)
            NotifyOtherParty(body)

            Response.Redirect(Request.RawUrl, True)
        End Sub

        ''' <summary>Email іншій стороні розмови (п.7 — та сама інфраструктура, що для модерації).
        ''' Помилка надсилання не повинна ламати саму відправку повідомлення.</summary>
        Private Sub NotifyOtherParty(body As String)
            Try
                If CurrentUserId = _consumerId Then
                    ' Написав споживач — сповіщаємо постачальника.
                    EmailSender.Send(_svc.ProviderEmail, "Нове повідомлення про ваше оголошення — Safina",
                        String.Format("Вам написали щодо оголошення «{0}»:{1}{1}{2}{1}{1}Відповісти можна на порталі, розділ «Повідомлення».",
                            _svc.Title, Environment.NewLine, body))
                Else
                    ' Написав постачальник — сповіщаємо споживача.
                    Dim consumer = UserAccount.GetById(_consumerId)
                    If consumer IsNot Nothing Then
                        EmailSender.Send(consumer.Email, "Нова відповідь щодо оголошення — Safina",
                            String.Format("Постачальник відповів щодо оголошення «{0}»:{1}{1}{2}{1}{1}Відповісти можна на порталі, розділ «Повідомлення».",
                                _svc.Title, Environment.NewLine, body))
                    End If
                End If
            Catch
                ' Дев-режим пише .eml на диск — падати тут може хіба через
                ' відсутність прав на App_Data, не хочемо через це ламати відправку.
            End Try
        End Sub

        Private Sub ShowNotFound()
            threadPanel.Visible = False
            notFoundPanel.Visible = True
        End Sub

    End Class

End Namespace
