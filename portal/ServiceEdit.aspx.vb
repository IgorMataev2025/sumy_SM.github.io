Imports System
Imports System.Globalization
Imports System.IO
Imports System.Web.UI.WebControls

Namespace SumyPortal

    Public Class ServiceEdit
        Inherits ProviderPageBase

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

                If ServiceIdParam > 0 Then
                    Dim svc = Service.GetById(ServiceIdParam, CurrentProvider.UserId)
                    If svc Is Nothing Then
                        Response.Redirect("~/MyServices.aspx", True)
                        Return
                    End If

                    headingLiteral.Text = "Редагування оголошення"
                    titleLiteral.Text = "Редагування оголошення"

                    If svc.Status <> "Draft" AndAlso svc.Status <> "Rejected" Then
                        formPanel.Visible = False
                        lockedPanel.Visible = True
                        lockedText.Text = If(svc.Status = "Pending",
                            "Оголошення вже подано на модерацію — редагування недоступне, доки адміністратор не прийме рішення.",
                            "Оголошення опубліковано. Щоб редагувати, спершу зніміть його з публікації в списку оголошень.")
                        Return
                    End If

                    ddlCategory.SelectedValue = svc.CategoryId.ToString()
                    txtTitle.Text = svc.Title
                    txtDescription.Text = svc.Description
                    txtPrice.Text = If(svc.Price.HasValue, svc.Price.Value.ToString("0.##", CultureInfo.InvariantCulture), String.Empty)
                    txtDistrict.Text = svc.District
                    txtPhone.Text = svc.Phone

                    BindPhotos(svc.ServiceId)
                End If
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

        Protected Sub btnSaveDraft_Click(sender As Object, e As EventArgs)
            Dim serviceId = SaveService()
            If serviceId.HasValue Then
                Response.Redirect("ServiceEdit.aspx?id=" & serviceId.Value)
            End If
        End Sub

        Protected Sub btnSubmitModeration_Click(sender As Object, e As EventArgs)
            Dim serviceId = SaveService()
            If serviceId.HasValue Then
                Service.SubmitForModeration(serviceId.Value, CurrentProvider.UserId)
                Response.Redirect("MyServices.aspx")
            End If
        End Sub

        ''' <summary>Зберігає поля, обробляє видалення позначених і завантаження нових фото. Nothing — якщо є помилка валідації/збереження.</summary>
        Private Function SaveService() As Integer?
            If Not Page.IsValid Then Return Nothing

            Dim categoryId = Integer.Parse(ddlCategory.SelectedValue)
            Dim title = txtTitle.Text.Trim()
            Dim description = txtDescription.Text.Trim()
            Dim price As Decimal? = Nothing
            If Not String.IsNullOrWhiteSpace(txtPrice.Text) Then
                price = Decimal.Parse(txtPrice.Text, CultureInfo.InvariantCulture)
            End If
            Dim district = txtDistrict.Text.Trim()
            Dim phone = txtPhone.Text.Trim()

            Dim serviceId = ServiceIdParam
            If serviceId = 0 Then
                serviceId = Service.Create(CurrentProvider.UserId, categoryId, title, description, price, district, phone)
            Else
                Dim updated = Service.Update(serviceId, CurrentProvider.UserId, categoryId, title, description, price, district, phone)
                If Not updated Then
                    ShowError("Не вдалося зберегти — оголошення вже на модерації або опубліковано.")
                    Return Nothing
                End If
            End If

            ProcessPhotoDeletions()
            ProcessPhotoUploads(serviceId)

            Return serviceId
        End Function

        Private Sub ProcessPhotoDeletions()
            For Each item As RepeaterItem In rptPhotos.Items
                Dim chk = TryCast(item.FindControl("chkDeletePhoto"), CheckBox)
                Dim hid = TryCast(item.FindControl("hidPhotoId"), HiddenField)
                If chk IsNot Nothing AndAlso chk.Checked AndAlso hid IsNot Nothing Then
                    Dim photoId As Integer
                    If Integer.TryParse(hid.Value, photoId) Then
                        Dim filePath = Service.DeletePhoto(photoId, CurrentProvider.UserId)
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

        Private Sub ShowError(message As String)
            serverErrorLabel.Text = message
            serverErrorLabel.Visible = True
        End Sub

    End Class

End Namespace
