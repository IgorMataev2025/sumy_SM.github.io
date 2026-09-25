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

        ''' <summary>Специфікація (наступна фіча понад MVP, реалізовано за прямим запитом
        ''' користувача, 2026-09-14) — рівно 0 або 1 файл, на відміну від фото (до MaxPhotos).</summary>
        Private Const MaxSpecFileSizeBytes As Integer = 5 * 1024 * 1024 ' 5 МБ
        Private Shared ReadOnly AllowedSpecExtensions As String() = {".xls", ".xlsx"}

        Private ReadOnly Property ServiceIdParam As Integer
            Get
                Dim id As Integer
                Integer.TryParse(Request.QueryString("id"), id)
                Return id
            End Get
        End Property

        Protected Overrides Sub InitializeCulture()
            LocalizationHelper.ApplyCulture(Me)
            MyBase.InitializeCulture()
        End Sub

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            maxPhotosLiteral.Text = MaxPhotos.ToString()

            If Not IsPostBack Then
                ' За замовчуванням — нове оголошення; нижче перевизначається для режиму
                ' редагування. Встановлюємо лише тут (не на кожному Page_Load), бо
                ' Literal.Text зберігається у ViewState — інакше постбек (валідація,
                ' завантаження фото) скидав би заголовок назад на "Нове оголошення".
                headingLiteral.Text = Resources.SiteText.ServiceEdit_Heading_New
                titleLiteral.Text = Resources.SiteText.ServiceEdit_Heading_New

                BindCategories()
                SumyDistricts.Populate(ddlDistrict, Resources.SiteText.District_NotSpecified)

                If ServiceIdParam > 0 Then
                    Dim svc = Service.GetById(ServiceIdParam, CurrentProvider.UserId)
                    If svc Is Nothing Then
                        Response.Redirect("~/MyServices.aspx", True)
                        Return
                    End If

                    headingLiteral.Text = Resources.SiteText.ServiceEdit_Heading_Edit
                    titleLiteral.Text = Resources.SiteText.ServiceEdit_Heading_Edit

                    If svc.Status = "Approved" Then
                        ' Опубліковане — швидке редагування ціни/району/телефону без модерації.
                        formPanel.Visible = False
                        quickEditPanel.Visible = True
                        quickTitleLiteral.Text = Server.HtmlEncode(svc.Title)
                        txtQuickPrice.Text = If(svc.Price.HasValue, svc.Price.Value.ToString("0.##", CultureInfo.InvariantCulture), String.Empty)
                        SumyDistricts.Populate(ddlQuickDistrict, Resources.SiteText.District_NotSpecified)
                        If ddlQuickDistrict.Items.FindByValue(svc.District) IsNot Nothing Then
                            ddlQuickDistrict.SelectedValue = svc.District
                        End If
                        txtQuickPhone.Text = svc.Phone
                        Return
                    End If

                    If svc.Status <> "Draft" AndAlso svc.Status <> "Rejected" Then
                        formPanel.Visible = False
                        lockedPanel.Visible = True
                        lockedText.Text = Resources.SiteText.ServiceEdit_Locked_Pending
                        Return
                    End If

                    ' Захист: адмін міг деактивувати категорію (AdminCategories.aspx) після
                    ' того, як це оголошення її отримало — ddlCategory показує лише активні
                    ' (GetActiveCategories), тому SelectedValue кине виняток без перевірки
                    ' (той самий прийом, що вже для District нижче).
                    If ddlCategory.Items.FindByValue(svc.CategoryId.ToString()) IsNot Nothing Then
                        ddlCategory.SelectedValue = svc.CategoryId.ToString()
                    End If
                    txtTitle.Text = svc.Title
                    txtDescription.Text = svc.Description
                    txtPrice.Text = If(svc.Price.HasValue, svc.Price.Value.ToString("0.##", CultureInfo.InvariantCulture), String.Empty)
                    ' Захист: старе оголошення могло зберегти район вільним текстом
                    ' (до впровадження довідника) — якщо значення не входить у
                    ' поточний список, SelectedValue кине виняток, тому перевіряємо.
                    If ddlDistrict.Items.FindByValue(svc.District) IsNot Nothing Then
                        ddlDistrict.SelectedValue = svc.District
                    End If
                    txtPhone.Text = svc.Phone
                    If svc.Latitude.HasValue AndAlso svc.Longitude.HasValue Then
                        hidLatitude.Value = svc.Latitude.Value.ToString(CultureInfo.InvariantCulture)
                        hidLongitude.Value = svc.Longitude.Value.ToString(CultureInfo.InvariantCulture)
                    End If

                    BindPhotos(svc.ServiceId)
                    BindSpecification(svc.SpecificationFilePath)
                End If
            End If
        End Sub

        ''' <summary>Показує ім'я поточного файлу специфікації + чекбокс видалення, і
        ''' розпарсований перегляд нижче (той самий SpecificationReader, що на
        ''' ServiceDetails.aspx.vb для споживача). Нічого не показує, якщо файл не
        ''' завантажено або його не вдалося прочитати (наприклад, пошкоджено вручну на диску).</summary>
        Private Sub BindSpecification(specPath As String)
            If String.IsNullOrEmpty(specPath) Then Return

            currentSpecPanel.Visible = True
            currentSpecFileName.Text = Server.HtmlEncode(Path.GetFileName(specPath))

            Dim html As String = Nothing
            If SpecificationReader.TryReadAsHtmlTable(Server.MapPath(specPath), html) Then
                specPreviewLiteral.Text = html
                specPreviewPanel.Visible = True
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
                If Service.SubmitForModeration(serviceId.Value, CurrentProvider.UserId) Then
                    AdminNotifier.NewPendingService(serviceId.Value, txtTitle.Text.Trim())
                End If
                Response.Redirect("MyServices.aspx")
            End If
        End Sub

        ''' <summary>«Зробити головним» — окремий постбек без валідації й без збереження решти
        ''' форми: введені, але не збережені поля переживають постбек у самих контролах.</summary>
        Protected Sub rptPhotos_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
            If e.CommandName <> "MakeMain" Then Return
            Dim photoId As Integer
            If Integer.TryParse(CStr(e.CommandArgument), photoId) Then
                Service.SetMainPhoto(photoId, CurrentProvider.UserId)
            End If
            BindPhotos(ServiceIdParam)
        End Sub

        Protected Sub btnQuickSave_Click(sender As Object, e As EventArgs)
            If Not Page.IsValid Then Return

            Dim price As Decimal? = Nothing
            If Not String.IsNullOrWhiteSpace(txtQuickPrice.Text) Then
                price = Decimal.Parse(txtQuickPrice.Text, CultureInfo.InvariantCulture)
            End If

            ' Якщо оголошення тим часом зняли з публікації/автозняли — SQL не оновить рядок.
            Dim ok = Service.UpdatePublishedDetails(ServiceIdParam, CurrentProvider.UserId,
                price, ddlQuickDistrict.SelectedValue, txtQuickPhone.Text.Trim())
            quickResultLabel.Text = If(ok, Resources.SiteText.ServiceEdit_Quick_Saved, Resources.SiteText.ServiceEdit_Quick_Failed)
            quickResultLabel.CssClass = If(ok, "stub-note", "form-error")
            quickResultLabel.Visible = True
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
            Dim district = ddlDistrict.SelectedValue
            Dim phone = txtPhone.Text.Trim()

            ' Геолокація (постановка робочої тестової версії, 2026-09-12) — необов'язкова,
            ' hidLatitude/hidLongitude заповнюються JS кліком на карті (ServiceEdit.aspx);
            ' порожні, якщо постачальник не ставив мітку.
            Dim latitude As Decimal? = Nothing
            Dim longitude As Decimal? = Nothing
            Dim parsedLat, parsedLng As Decimal
            If Decimal.TryParse(hidLatitude.Value, NumberStyles.Float, CultureInfo.InvariantCulture, parsedLat) AndAlso
               Decimal.TryParse(hidLongitude.Value, NumberStyles.Float, CultureInfo.InvariantCulture, parsedLng) Then
                latitude = parsedLat
                longitude = parsedLng
            End If

            Dim serviceId = ServiceIdParam
            If serviceId = 0 Then
                serviceId = Service.Create(CurrentProvider.UserId, categoryId, title, description, price, district, phone, latitude, longitude)
            Else
                Dim updated = Service.Update(serviceId, CurrentProvider.UserId, categoryId, title, description, price, district, phone, latitude, longitude)
                If Not updated Then
                    ShowError(Resources.SiteText.ServiceEdit_Err_LockedSave)
                    Return Nothing
                End If
            End If

            ProcessPhotoDeletions()
            ProcessPhotoUploads(serviceId)

            ' На відміну від ProcessPhotoUploads (мовчки пропускає файл з невірним форматом/
            ' розміром — фото до 5, помилка не критична), специфікація — рівно 0/1 файл: якщо
            ' постачальник саме зараз намагався його прикріпити й помилився форматом/розміром,
            ' Return Nothing зупиняє SaveService ДО btnSaveDraft_Click/btnSubmitModeration_Click
            ' Response.Redirect — інакше ShowError() усередині ProcessSpecificationUpload
            ' рендериться в тіло відповіді, яку Redirect одразу відкидає (Response.End),
            ' і постачальник ніколи не побачить, чому файл не додався. Той самий принцип,
            ' що вже ServiceId=0-перевірка Update() вище (ShowError + Return Nothing).
            If Not ProcessSpecificationUpload(serviceId) Then Return Nothing

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
                    ShowError(String.Format(Resources.SiteText.Photo_Err_MaxCount, MaxPhotos))
                    Exit For
                End If

                Dim ext = Path.GetExtension(posted.FileName).ToLowerInvariant()
                If Array.IndexOf(AllowedExtensions, ext) < 0 Then
                    ShowError(Resources.SiteText.Photo_Err_BadFormat)
                    Continue For
                End If
                If posted.ContentLength > MaxFileSizeBytes Then
                    ShowError(Resources.SiteText.Photo_Err_TooLarge)
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

        ''' <summary>Специфікація (наступна фіча понад MVP, реалізовано за прямим запитом
        ''' користувача, 2026-09-14) — рівно 0 або 1 файл: чекбокс видалення обробляється
        ''' першим (той самий принцип, що ProcessPhotoDeletions), потім нове завантаження
        ''' (якщо є) заміняє старе — попередній файл прибирається з диска перед записом нового
        ''' шляху, щоб не лишати сирітські файли в Uploads/Services/{id}/. False — постачальник
        ''' щойно намагався прикріпити файл і помилився форматом/розміром/файл пошкоджений;
        ''' викликач (SaveService) зупиняє збереження до Response.Redirect, інакше повідомлення
        ''' ShowError() ніколи не дійде до браузера (Response.End у Redirect відкидає тіло
        ''' поточної відповіді).</summary>
        Private Function ProcessSpecificationUpload(serviceId As Integer) As Boolean
            Dim currentPath = Service.GetSpecificationFilePath(serviceId, CurrentProvider.UserId)

            If chkRemoveSpecification.Checked AndAlso Not String.IsNullOrEmpty(currentPath) Then
                DeleteSpecificationFile(currentPath)
                Service.SetSpecificationFilePath(serviceId, CurrentProvider.UserId, Nothing)
                currentPath = Nothing
            End If

            If Not fileSpecification.HasFile Then Return True

            Dim posted = fileSpecification.PostedFile
            Dim ext = Path.GetExtension(posted.FileName).ToLowerInvariant()
            If Array.IndexOf(AllowedSpecExtensions, ext) < 0 Then
                ShowError(Resources.SiteText.ServiceEdit_Specification_Err_Format)
                Return False
            End If
            If posted.ContentLength > MaxSpecFileSizeBytes Then
                ShowError(Resources.SiteText.ServiceEdit_Specification_Err_TooLarge)
                Return False
            End If

            If Not String.IsNullOrEmpty(currentPath) Then DeleteSpecificationFile(currentPath)

            Dim fileName = Guid.NewGuid().ToString("N") & ext
            Dim relativeDir = "~/Uploads/Services/" & serviceId & "/"
            Dim physicalDir = Server.MapPath(relativeDir)
            If Not Directory.Exists(physicalDir) Then Directory.CreateDirectory(physicalDir)
            Dim relativePath = relativeDir & fileName
            posted.SaveAs(Path.Combine(physicalDir, fileName))

            ' Швидка перевірка одразу після завантаження — файл насправді пошкоджений/не Excel
            ' (найменування .xlsx нічого не гарантує). Якщо не читається — не записуємо шлях у
            ' БД і прибираємо файл з диска, повідомляємо постачальника одразу, а не лишаємо
            ' його дізнаватись про це лише коли попередній перегляд мовчки не з'явиться.
            Dim html As String = Nothing
            If Not SpecificationReader.TryReadAsHtmlTable(Path.Combine(physicalDir, fileName), html) Then
                DeleteSpecificationFile(relativePath)
                ShowError(Resources.SiteText.ServiceEdit_Specification_Err_ParseFailed)
                Return False
            End If

            Service.SetSpecificationFilePath(serviceId, CurrentProvider.UserId, relativePath)
            Return True
        End Function

        Private Sub DeleteSpecificationFile(relativePath As String)
            Try
                Dim physicalPath = Server.MapPath(relativePath)
                If File.Exists(physicalPath) Then File.Delete(physicalPath)
            Catch
                ' Файл на диску вже відсутній — не критично (той самий принцип, що DeletePhotoFile).
            End Try
        End Sub

        Private Sub ShowError(message As String)
            serverErrorLabel.Text = message
            serverErrorLabel.Visible = True
        End Sub

    End Class

End Namespace
