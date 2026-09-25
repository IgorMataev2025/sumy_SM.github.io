Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Web.UI.WebControls

Namespace SumyPortal

    Public Class AdminModeration
        Inherits AdminPageBase

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not IsPostBack Then
                BindQueue()
            End If
        End Sub

        ''' <summary>Фото оголошення для картки черги (кеш на запит — розмітка питає двічі:
        ''' для мініатюр і для позначки "Фото немає").</summary>
        Private ReadOnly _photosCache As New Dictionary(Of Integer, List(Of ServicePhoto))

        Protected Function PhotosOf(dataItem As Object) As List(Of ServicePhoto)
            Dim serviceId = CType(dataItem, Service).ServiceId
            Dim photos As List(Of ServicePhoto) = Nothing
            If Not _photosCache.TryGetValue(serviceId, photos) Then
                photos = Service.GetPhotos(serviceId)
                _photosCache(serviceId) = photos
            End If
            Return photos
        End Function

        Protected Function MapUrl(dataItem As Object) As String
            Dim svc = CType(dataItem, Service)
            If Not svc.Latitude.HasValue OrElse Not svc.Longitude.HasValue Then Return String.Empty
            Dim lat = svc.Latitude.Value.ToString("0.######", CultureInfo.InvariantCulture)
            Dim lng = svc.Longitude.Value.ToString("0.######", CultureInfo.InvariantCulture)
            Return String.Format("https://www.openstreetmap.org/?mlat={0}&mlon={1}#map=16/{0}/{1}", lat, lng)
        End Function

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
                    ' Типова причина зі списку + (необов'язково) власне уточнення — 2026-09-25.
                    Dim ddlTemplate = TryCast(e.Item.FindControl("ddlRejectTemplate"), DropDownList)
                    Dim txtReason = TryCast(e.Item.FindControl("txtRejectReason"), TextBox)
                    Dim parts As New List(Of String)
                    If ddlTemplate IsNot Nothing AndAlso Not String.IsNullOrEmpty(ddlTemplate.SelectedValue) Then parts.Add(ddlTemplate.SelectedValue)
                    If txtReason IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(txtReason.Text) Then parts.Add(txtReason.Text.Trim())
                    Dim reason = String.Join(". ", parts)

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
