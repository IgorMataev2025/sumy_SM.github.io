Imports System
Imports System.Collections.Generic

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

        Protected Overrides Sub InitializeCulture()
            LocalizationHelper.ApplyCulture(Me)
            MyBase.InitializeCulture()
        End Sub

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

            ' Server.HtmlEncode — Title вільний текст постачальника, сторінка бачить
            ' сесію співрозмовника (той самий прийом, що вже в ServiceDetails.aspx.vb).
            headingLiteral.Text = Server.HtmlEncode(_svc.Title)

            If Not IsPostBack Then
                BindThread()
            End If
        End Sub

        Private Sub BindThread()
            Dim thread = DialogMessage.GetThread(_serviceId, _consumerId)
            rptMessages.DataSource = thread
            rptMessages.DataBind()
            noMessagesPanel.Visible = (thread.Count = 0)

            ' Структурована заявка — лише коли розмова ще порожня і пише сам споживач
            ' (не постачальник, що першим би "відповідав" у ще не початій розмові —
            ' теоретично неможливо, бо кнопка на ServiceDetails.aspx і так лише для
            ' Consumer, але перевіряємо явно, а не покладаємось на це побічно).
            orderFieldsPanel.Visible = (thread.Count = 0) AndAlso (CurrentUserId = _consumerId)
            bodyLabelLiteral.Text = If(orderFieldsPanel.Visible, Resources.SiteText.MessageThread_Label_Details, Resources.SiteText.MessageThread_Label_Message)
        End Sub

        Protected Sub btnSend_Click(sender As Object, e As EventArgs)
            Dim comment = txtBody.Text.Trim()
            If String.IsNullOrEmpty(comment) Then
                errorLabel.Text = Resources.SiteText.MessageThread_Err_EmptyBody
                errorLabel.Visible = True
                Return
            End If

            Dim body = comment
            If orderFieldsPanel.Visible Then
                ' Дата/адреса об'єднуються в текст першого повідомлення — окремої
                ' сутності/статусу заявки немає (рішення користувача, 2026-09-12).
                Dim details As New List(Of String)
                If Not String.IsNullOrWhiteSpace(txtDesiredDate.Text) Then
                    details.Add(Resources.SiteText.MessageThread_DesiredDatePrefix & txtDesiredDate.Text.Trim())
                End If
                If Not String.IsNullOrWhiteSpace(txtAddress.Text) Then
                    details.Add(Resources.SiteText.MessageThread_AddressPrefix & txtAddress.Text.Trim())
                End If
                If details.Count > 0 Then
                    body = String.Join(Environment.NewLine, details) & Environment.NewLine & Environment.NewLine & comment
                End If
            End If

            ' Messages.Body VARCHAR(2000) — дата+адреса можуть додати понад ліміт
            ' txtBody (теж 2000), тому підрізаємо, а не покладаємось на MySQL-помилку.
            If body.Length > 2000 Then body = body.Substring(0, 2000)

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
