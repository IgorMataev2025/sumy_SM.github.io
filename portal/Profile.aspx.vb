Imports System
Imports System.IO
Imports System.Web.Security
Imports System.Web.UI.WebControls
Imports MySql.Data.MySqlClient

Namespace SumyPortal

    ''' <summary>
    ''' Особистий кабінет (п.6 уточненої постановки) — редагування власних даних
    ''' і самостійне видалення акаунта. Доступний будь-якому залогіненому
    ''' користувачу (Consumer і Provider).
    '''
    ''' Постановка робочої тестової версії (2026-09-12, варіант 1): та сама сторінка
    ''' тепер має ДВА режими. Без "?providerId=" — це "мій профіль" (як і раніше,
    ''' вимагає входу). З "?providerId=X" — публічний перегляд галереї постачальника
    ''' X (доступний і неавторизованим — Web.config відкриває сторінку через
    ''' &lt;location&gt;, той самий прийом, що вже для ServiceDetails.aspx); форма
    ''' редагування й видалення акаунта в цьому режимі не рендеряться взагалі
    ''' (Visible=False), крім випадку, коли providerId — це власний Id відвідувача.
    ''' </summary>
    Public Class Profile
        Inherits System.Web.UI.Page

        Private Const MaxGalleryPhotos As Integer = 5
        Private Const MaxFileSizeBytes As Integer = 3 * 1024 * 1024 ' 3 МБ
        Private Shared ReadOnly AllowedExtensions As String() = {".jpg", ".jpeg", ".png", ".gif"}

        ''' <summary>Профіль, що показується — власний або чужий (публічний перегляд).</summary>
        Private CurrentUser As UserAccount

        ''' <summary>True — власник дивиться свій кабінет (форма редагування, керування галереєю).
        ''' False — сторонній/анонімний відвідувач бачить лише публічну галерею.</summary>
        Private IsOwnProfile As Boolean

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            Dim loggedInUser = UserAccount.FindByEmail(Page.User.Identity.Name)

            Dim providerIdParam As Integer
            Dim hasProviderIdParam = Integer.TryParse(Request.QueryString("providerId"), providerIdParam)

            If hasProviderIdParam Then
                If loggedInUser IsNot Nothing AndAlso loggedInUser.UserId = providerIdParam Then
                    ' Власне посилання (напр. відкрив свою картку з ServiceDetails.aspx) — повний режим,
                    ' незалежно від UserType (щоб не ламати випадок, якби Consumer сам дописав параметр).
                    IsOwnProfile = True
                    CurrentUser = loggedInUser
                Else
                    Dim target = UserAccount.GetById(providerIdParam)
                    ' Лише активні Provider-акаунти публічні — заблокований/самовидалений
                    ' (IsActive=False після UserAccount.DeleteAccount) не витікає назовні.
                    If target Is Nothing OrElse target.UserType <> "Provider" OrElse Not target.IsActive Then
                        formPanel.Visible = False
                        dangerZonePanel.Visible = False
                        galleryPanel.Visible = False
                        notFoundPanel.Visible = True
                        Return
                    End If
                    IsOwnProfile = False
                    CurrentUser = target
                End If
            Else
                ' Без providerId — це "мій профіль", вхід обов'язковий (як і раніше, 2026-09-08).
                If loggedInUser Is Nothing Then
                    Response.Redirect("~/Default.aspx", True)
                    Return
                End If
                IsOwnProfile = True
                CurrentUser = loggedInUser
            End If

            headingLiteral.Text = If(IsOwnProfile, "Мій профіль", "Профіль постачальника: " & Server.HtmlEncode(CurrentUser.FullName))
            formPanel.Visible = IsOwnProfile
            publicPanel.Visible = Not IsOwnProfile

            If Not IsPostBack Then
                If IsOwnProfile Then
                    SumyDistricts.Populate(ddlDistrict, "Не вказано")
                    BindProfile()
                Else
                    publicNameLiteral.Text = Server.HtmlEncode(
                        If(CurrentUser.IsLegalEntity AndAlso Not String.IsNullOrWhiteSpace(CurrentUser.CompanyName),
                           CurrentUser.CompanyName, CurrentUser.FullName))
                    ' Самовидалення/блокування недоступні в публічному режимі — панель ховаємо явно,
                    ' а не покладаємось на те, що formPanel.Visible=False її вже приховує (danger-zone —
                    ' окрема панель у розмітці, поза formPanel).
                    dangerZonePanel.Visible = False
                End If

                BindGallery()
            End If
        End Sub

        Private Sub BindProfile()
            emailLiteral.Text = Server.HtmlEncode(CurrentUser.Email)
            txtFullName.Text = CurrentUser.FullName
            txtPhone.Text = CurrentUser.Phone

            If ddlDistrict.Items.FindByValue(CurrentUser.District) IsNot Nothing Then
                ddlDistrict.SelectedValue = CurrentUser.District
            End If

            legalEntityPanel.Visible = CurrentUser.IsLegalEntity
            If CurrentUser.IsLegalEntity Then
                txtCompanyName.Text = CurrentUser.CompanyName
                txtEdrpou.Text = CurrentUser.EDRPOU
            End If

            ' Самовидалення адміна не передбачено (UserAccount.DeleteAccount це й
            ' блокує на рівні SQL) — ховаємо кнопку, а не показуємо помилку постфактум.
            dangerZonePanel.Visible = Not CurrentUser.IsAdmin
        End Sub

        Protected Sub btnSave_Click(sender As Object, e As EventArgs)
            ' Захист від підробленого postback: контрол лишається в дереві сторінки навіть коли
            ' formPanel.Visible=False (публічний перегляд), тому __EVENTTARGET можна підробити
            ' напряму в POST — перевіряємо роль ще раз тут, а не лише ховаємо кнопку в розмітці.
            If Not IsOwnProfile Then Return
            If Not Page.IsValid Then Return

            Dim companyName = If(CurrentUser.IsLegalEntity, txtCompanyName.Text.Trim(), Nothing)
            Dim edrpou = If(CurrentUser.IsLegalEntity, txtEdrpou.Text.Trim(), Nothing)

            If CurrentUser.IsLegalEntity AndAlso String.IsNullOrWhiteSpace(companyName) Then
                ShowError("Вкажіть назву компанії.")
                Return
            End If
            If CurrentUser.IsLegalEntity AndAlso String.IsNullOrWhiteSpace(edrpou) Then
                ShowError("Вкажіть ЄДРПОУ.")
                Return
            End If

            Try
                UserAccount.UpdateProfile(CurrentUser.UserId, txtFullName.Text.Trim(), txtPhone.Text.Trim(),
                                           ddlDistrict.SelectedValue, companyName, edrpou)
                infoLabel.Text = "Дані збережено."
                infoLabel.Visible = True
                CurrentUser = UserAccount.GetById(CurrentUser.UserId)
                BindProfile()
            Catch ex As MySqlException
                ShowError("Не вдалося зберегти — спробуйте пізніше.")
            End Try
        End Sub

        Protected Sub btnDeleteAccount_Click(sender As Object, e As EventArgs)
            If Not IsOwnProfile Then Return ' той самий захист, що btnSave_Click

            If UserAccount.DeleteAccount(CurrentUser.UserId) Then
                FormsAuthentication.SignOut()
                Response.Redirect("~/Default.aspx", True)
            Else
                ShowError("Не вдалося видалити акаунт — спробуйте пізніше.")
            End If
        End Sub

        ' --- Галерея на акаунті (постановка робочої тестової версії, 2026-09-12) ---
        ' Окремо від фото оголошень (ServicePhotos/Service.vb) — не прив'язана до
        ' конкретного оголошення, публічна (бачить будь-хто, включно з анонімами).

        Private Sub BindGallery()
            maxGalleryPhotosLiteral.Text = MaxGalleryPhotos.ToString()

            Dim photos = ProviderGalleryPhoto.GetByProvider(CurrentUser.UserId)
            rptGallery.DataSource = photos
            rptGallery.DataBind()
            noGalleryPhotosPanel.Visible = (photos.Count = 0)
            galleryUploadPanel.Visible = IsOwnProfile
        End Sub

        Protected Sub rptGallery_ItemDataBound(sender As Object, e As RepeaterItemEventArgs)
            If e.Item.ItemType <> ListItemType.Item AndAlso e.Item.ItemType <> ListItemType.AlternatingItem Then Return
            Dim deleteLink = TryCast(e.Item.FindControl("lnkDeleteGalleryPhoto"), LinkButton)
            If deleteLink IsNot Nothing Then deleteLink.Visible = IsOwnProfile
        End Sub

        Protected Sub rptGallery_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
            If Not IsOwnProfile Then Return ' той самий захист, що btnSave_Click — контрол є в дереві й для публічного перегляду

            If e.CommandName = "Delete" Then
                Dim photoId = Convert.ToInt32(e.CommandArgument)
                Dim filePath = ProviderGalleryPhoto.Delete(photoId, CurrentUser.UserId)
                If filePath IsNot Nothing Then DeleteGalleryPhotoFile(filePath)
            End If

            BindGallery()
        End Sub

        Protected Sub btnUploadGalleryPhoto_Click(sender As Object, e As EventArgs)
            If Not IsOwnProfile Then Return ' той самий захист, що btnSave_Click

            If galleryFileUpload.HasFiles Then
                Dim currentCount = ProviderGalleryPhoto.Count(CurrentUser.UserId)

                For Each posted In galleryFileUpload.PostedFiles
                    If posted Is Nothing OrElse posted.ContentLength = 0 Then Continue For

                    If currentCount >= MaxGalleryPhotos Then
                        ShowGalleryError(String.Format("Можна додати не більше {0} фото — частину файлів не збережено.", MaxGalleryPhotos))
                        Exit For
                    End If

                    Dim ext = Path.GetExtension(posted.FileName).ToLowerInvariant()
                    If Array.IndexOf(AllowedExtensions, ext) < 0 Then
                        ShowGalleryError("Дозволені формати фото: jpg, png, gif.")
                        Continue For
                    End If
                    If posted.ContentLength > MaxFileSizeBytes Then
                        ShowGalleryError("Розмір одного фото не повинен перевищувати 3 МБ.")
                        Continue For
                    End If

                    Dim fileName = Guid.NewGuid().ToString("N") & ext
                    Dim relativeDir = "~/Uploads/Gallery/" & CurrentUser.UserId & "/"
                    Dim physicalDir = Server.MapPath(relativeDir)
                    If Not Directory.Exists(physicalDir) Then Directory.CreateDirectory(physicalDir)
                    posted.SaveAs(Path.Combine(physicalDir, fileName))

                    ProviderGalleryPhoto.Add(CurrentUser.UserId, relativeDir & fileName)
                    currentCount += 1
                Next
            End If

            BindGallery()
        End Sub

        Private Sub DeleteGalleryPhotoFile(relativePath As String)
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

        Private Sub ShowGalleryError(message As String)
            galleryErrorLabel.Text = message
            galleryErrorLabel.Visible = True
        End Sub

    End Class

End Namespace
