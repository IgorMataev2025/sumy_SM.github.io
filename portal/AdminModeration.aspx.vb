Imports System
Imports System.Web.UI.WebControls

Namespace SumyPortal

    Public Class AdminModeration
        Inherits AdminPageBase

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not IsPostBack Then
                BindQueue()
            End If
        End Sub

        Private Sub BindQueue()
            Dim queue = Service.GetPendingForModeration()
            rptQueue.DataSource = queue
            rptQueue.DataBind()
            emptyPanel.Visible = (queue.Count = 0)
        End Sub

        Protected Sub rptQueue_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
            Dim serviceId = Convert.ToInt32(e.CommandArgument)
            ' Знімається ДО зміни статусу — GetPendingById бачить тільки Pending.
            Dim svc = Service.GetPendingById(serviceId)

            Select Case e.CommandName
                Case "Approve"
                    If Service.Approve(serviceId, CurrentAdmin.UserId) Then
                        ShowInfo("Оголошення опубліковано.")
                        NotifyProvider(svc, approved:=True, reason:=Nothing)
                    End If

                Case "Reject"
                    Dim txtReason = TryCast(e.Item.FindControl("txtRejectReason"), TextBox)
                    Dim reason = If(txtReason IsNot Nothing, txtReason.Text.Trim(), String.Empty)

                    If String.IsNullOrEmpty(reason) Then
                        ShowInfo("Для відхилення потрібно вказати причину.")
                    ElseIf Service.Reject(serviceId, CurrentAdmin.UserId, reason) Then
                        ShowInfo("Оголошення відхилено.")
                        NotifyProvider(svc, approved:=False, reason:=reason)
                    End If
            End Select

            BindQueue()
        End Sub

        ''' <summary>
        ''' Email постачальнику про результат модерації (п.7 уточненої постановки).
        ''' Помилка надсилання не повинна ламати саму модерацію — адмін уже бачить
        ''' результат дії на екрані (ShowInfo) незалежно від долі листа.
        ''' </summary>
        Private Sub NotifyProvider(svc As Service, approved As Boolean, reason As String)
            If svc Is Nothing Then Return
            Try
                If approved Then
                    EmailSender.Send(svc.ProviderEmail, "Ваше оголошення опубліковано — Safina",
                        String.Format("Вітаємо! Ваше оголошення «{0}» пройшло модерацію й опубліковано на порталі Safina.", svc.Title))
                Else
                    EmailSender.Send(svc.ProviderEmail, "Ваше оголошення відхилено — Safina",
                        String.Format("Ваше оголошення «{0}» відхилено адміністратором.{1}Причина: {2}",
                            svc.Title, Environment.NewLine, reason))
                End If
            Catch
                ' Дев-режим пише .eml на диск (EmailSender.vb) — падати тут може
                ' хіба через відсутність прав на App_Data, не хочемо через це
                ' ламати модерацію.
            End Try
        End Sub

        Private Sub ShowInfo(message As String)
            infoLabel.Text = message
            infoLabel.Visible = True
        End Sub

    End Class

End Namespace
