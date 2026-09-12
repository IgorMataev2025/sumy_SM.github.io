Imports System
Imports System.Globalization
Imports System.IO
Imports System.Web.UI.WebControls

Namespace SumyPortal

    ''' <summary>
    ''' Повний CRUD адміна над одним оголошенням (п.12 уточненої постановки,
    ''' 2026-09-11) — на відміну від ServiceEdit.aspx (постачальник, лише
    ''' власне оголошення, лише поки Draft/Rejected) тут можна редагувати будь-яке
    ''' оголошення в будь-якому статусі, напряму змінювати сам статус, і видаляти
    ''' відгуки на нього.
    ''' </summary>
    Public Class AdminServiceEdit
        Inherits AdminPageBase

        Private Const MaxPhotos As Integer = 5
        Private Const MaxFileSizeBytes As Integer = 3 * 1024 * 1024 ' 3 МБ
        Private Shared ReadOnly AllowedExtensions As String() = {".jpg", ".jpeg", ".png", ".gif"}

        Private ReadOnly Property ServiceIdParam As Integer
            Get
                Dim id As Integer
                Integer.TryParse(Request.QueryString("id"), id)
                Return id
            End Get
        End Property

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            maxPhotosLiteral.Text = MaxPhotos.ToString()

            If Not IsPostBack Then
                BindCategories()
                SumyDistricts.Populate(ddlDistrict, "Не вказано")

                Dim svc = Service.GetByIdAny(ServiceIdParam)
                If svc Is Nothing Then
                    formPanel.Visible = False
                    notFoundPanel.Visible = True
                    Return
                End If

                ' Server.HtmlEncode — Literal рендерить сирий HTML, а ProviderName/ProviderEmail
                ' — вільний текст постачальника; сторінка бачить сесію адміна (той самий
                ' прийом, що вже в ServiceContract.aspx.vb).
                providerLiteral.Text = String.Format("Постачальник: {0} ({1})",
                    Server.HtmlEncode(svc.ProviderName), Server.HtmlEncode(svc.ProviderEmail))

                ' Захист: ddlCategory показує лише активні категорії (GetActiveCategories),
                ' а це оголошення могло отримати категорію до її деактивації —
                ' SelectedValue кине виняток без перевірки (той самий прийом, що вже для
                ' District нижче, і що вже виправлено для ServiceEdit.aspx.vb постачальника).
                If ddlCategory.Items.FindByValue(svc.CategoryId.ToString()) IsNot Nothing Then
                    ddlCategory.SelectedValue = svc.CategoryId.ToString()
                End If
                txtTitle.Text = svc.Title
                txtDescription.Text = svc.Description
                txtPrice.Text = If(svc.Price.HasValue, svc.Price.Value.ToString("0.##", CultureInfo.InvariantCulture), String.Empty)
                If ddlDistrict.Items.FindByValue(svc.District) IsNot Nothing Then
                    ddlDistrict.SelectedValue = svc.District
                End If
                txtPhone.Text = svc.Phone
                If svc.Latitude.HasValue Then txtLatitude.Text = svc.Latitude.Value.ToString(CultureInfo.InvariantCulture)
                If svc.Longitude.HasValue Then txtLongitude.Text = svc.Longitude.Value.ToString(CultureInfo.InvariantCulture)
                ddlStatus.SelectedValue = svc.Status
                txtRejectReason.Text = svc.RejectReason
                rejectReasonPanel.Visible = (svc.Status = "Rejected")
                chkVerified.Checked = svc.IsVerified

                BindPhotos(svc.ServiceId)
                BindReviews(svc.ServiceId)
            End If
        End Sub

        Private Sub BindCategories()
            ddlCategory.DataSource = ServiceCategory.GetActiveCategories()
            ddlCategory.DataBind()
        End Sub

        Private Sub BindPhotos(serviceId As Integer)
            Dim photos = Service.GetPhotos(serviceId)
            rptPhotos.DataSource = photos
            rptPhotos.DataBind()
            existingPhotosPanel.Visible = photos.Count > 0
        End Sub

        Private Sub BindReviews(serviceId As Integer)
            Dim reviews = Review.GetByService(serviceId)
            rptReviews.DataSource = reviews
            rptReviews.DataBind()
            noReviewsPanel.Visible = (reviews.Count = 0)
            reviewsPanel.Visible = True
        End Sub

        Protected Sub ddlStatus_SelectedIndexChanged(sender As Object, e As EventArgs)
            rejectReasonPanel.Visible = (ddlStatus.SelectedValue = "Rejected")
        End Sub

        Protected Sub btnSave_Click(sender As Object, e As EventArgs)
            If Not Page.IsValid Then Return

            Dim svc = Service.GetByIdAny(ServiceIdParam)
            If svc Is Nothing Then Return

            Dim categoryId = Integer.Parse(ddlCategory.SelectedValue)
            Dim title = txtTitle.Text.Trim()
            Dim description = txtDescription.Text.Trim()
            Dim price As Decimal? = Nothing
            If Not String.IsNullOrWhiteSpace(txtPrice.Text) Then
                price = Decimal.Parse(txtPrice.Text, CultureInfo.InvariantCulture)
            End If
            Dim district = ddlDistrict.SelectedValue
            Dim phone = txtPhone.Text.Trim()
            Dim latitude As Decimal? = Nothing
            If Not String.IsNullOrWhiteSpace(txtLatitude.Text) Then
                latitude = Decimal.Parse(txtLatitude.Text, CultureInfo.InvariantCulture)
            End If
            Dim longitude As Decimal? = Nothing
            If Not String.IsNullOrWhiteSpace(txtLongitude.Text) Then
                longitude = Decimal.Parse(txtLongitude.Text, CultureInfo.InvariantCulture)
            End If
            Dim status = ddlStatus.SelectedValue
            Dim rejectReason = If(status = "Rejected", txtRejectReason.Text.Trim(), Nothing)

            ' Той самий захист, що вже в AdminModeration.aspx.vb — без причини провайдер
            ' бачить "Відхилено" без пояснення (MyServices.aspx рядок причини лише
            ' Visible, коли RejectReason не порожній).
            If status = "Rejected" AndAlso String.IsNullOrEmpty(rejectReason) Then
                ShowError("Для статусу ""Відхилено"" потрібно вказати причину.")
                Return
            End If

            Dim updated = Service.AdminUpdate(svc.ServiceId, categoryId, title, description, price, district, phone, latitude, longitude, status, rejectReason, chkVerified.Checked)
            If Not updated Then
                ShowError("Не вдалося зберегти оголошення.")
                Return
            End If

            ProcessPhotoDeletions()
            ProcessPhotoUploads(svc.ServiceId)

            AdminActionLog.Log(CurrentAdmin.UserId, "Редагував оголошення",
                String.Format("""{0}"" (постачальник {1})", title, svc.ProviderEmail))

            BindPhotos(svc.ServiceId)
            BindReviews(svc.ServiceId)
            ShowInfo("Збережено.")
        End Sub

        Private Sub ProcessPhotoDeletions()
            For Each item As RepeaterItem In rptPhotos.Items
                Dim chk = TryCast(item.FindControl("chkDeletePhoto"), CheckBox)
                Dim hid = TryCast(item.FindControl("hidPhotoId"), HiddenField)
                If chk IsNot Nothing AndAlso chk.Checked AndAlso hid IsNot Nothing Then
                    Dim photoId As Integer
                    If Integer.TryParse(hid.Value, photoId) Then
                        Dim filePath = Service.AdminDeletePhoto(photoId)
                        If filePath IsNot Nothing Then DeletePhotoFile(filePath)
                    End If
                End If
            Next
        End Sub

        Private Sub ProcessPhotoUploads(serviceId As Integer)
            If Not fileUpload.HasFiles Then Return

            Dim currentCount = Service.CountPhotos(serviceId)

            For Each posted In fileUpload.PostedFiles
                If posted Is Nothing OrElse posted.ContentLength = 0 Then Continue For

                If currentCount >= MaxPhotos Then
                    ShowError(String.Format("Можна додати не більше {0} фото — частину файлів не збережено.", MaxPhotos))
                    Exit For
                End If

                Dim ext = Path.GetExtension(posted.FileName).ToLowerInvariant()
                If Array.IndexOf(AllowedExtensions, ext) < 0 Then
                    ShowError("Дозволені формати фото: jpg, png, gif.")
                    Continue For
                End If
                If posted.ContentLength > MaxFileSizeBytes Then
                    ShowError("Розмір одного фото не повинен перевищувати 3 МБ.")
                    Continue For
                End If

                Dim fileName = Guid.NewGuid().ToString("N") & ext
                Dim relativeDir = "~/Uploads/Services/" & serviceId & "/"
                Dim physicalDir = Server.MapPath(relativeDir)
                If Not Directory.Exists(physicalDir) Then Directory.CreateDirectory(physicalDir)
                posted.SaveAs(Path.Combine(physicalDir, fileName))

                Service.AddPhoto(serviceId, relativeDir & fileName)
                currentCount += 1
            Next
        End Sub

        Private Sub DeletePhotoFile(relativePath As String)
            Try
                Dim physicalPath = Server.MapPath(relativePath)
                If File.Exists(physicalPath) Then File.Delete(physicalPath)
            Catch
                ' Файл на диску не видалено (напр. вже відсутній) — запис у БД вже прибрано, це не критично.
            End Try
        End Sub

        Protected Sub rptReviews_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
            If e.CommandName = "Delete" Then
                Dim reviewId = Convert.ToInt32(e.CommandArgument)
                ' Назва оголошення — з БД, а не з txtTitle.Text: на постбеку контрол
                ' надійний лише якщо форма прийшла повністю, тоді як тут потрібен
                ' лише незалежний від цього запис у журналі.
                Dim svc = Service.GetByIdAny(ServiceIdParam)
                If Review.AdminDelete(reviewId) Then
                    AdminActionLog.Log(CurrentAdmin.UserId, "Видалив відгук",
                        String.Format("Відгук #{0} на оголошення ""{1}""", reviewId, If(svc IsNot Nothing, svc.Title, ServiceIdParam.ToString())))
                    ShowInfo("Відгук видалено.")
                End If
            End If

            BindReviews(ServiceIdParam)
        End Sub

        Private Sub ShowError(message As String)
            serverErrorLabel.Text = message
            serverErrorLabel.Visible = True
        End Sub

        Private Sub ShowInfo(message As String)
            infoLabel.Text = message
            infoLabel.Visible = True
        End Sub

    End Class

End Namespace
