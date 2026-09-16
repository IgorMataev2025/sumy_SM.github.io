Imports System
Imports System.IO

Namespace SumyPortal

    Public Class AdminUsers
        Inherits AdminPageBase

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not IsPostBack Then
                BindUsers()
            End If
        End Sub

        Private Sub BindUsers()
            rptUsers.DataSource = UserAccount.GetAll()
            rptUsers.DataBind()
        End Sub

        Protected Sub rptUsers_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
            Dim userId = Convert.ToInt32(e.CommandArgument)
            Dim target = UserAccount.GetAll().Find(Function(u) u.UserId = userId)
            If target Is Nothing Then Return

            Select Case e.CommandName
                Case "ToggleActive"
                    If UserAccount.SetActive(userId, Not target.IsActive) Then
                        ShowInfo(If(target.IsActive, "Користувача заблоковано.", "Користувача розблоковано."))
                        AdminActionLog.Log(CurrentAdmin.UserId, If(target.IsActive, "Заблокував користувача", "Розблокував користувача"), target.Email)
                    End If

                Case "Delete"
                    DeleteUser(target)
            End Select

            BindUsers()
        End Sub

        ''' <summary>Знімаємо шляхи до файлів ДО видалення рядків з БД — ServicePhotos/
        ''' ProviderGalleryPhotos прибираються каскадно разом з оголошеннями/користувачем
        ''' (ON DELETE CASCADE, schema.sql), той самий принцип, що AdminServices.aspx.vb.</summary>
        Private Sub DeleteUser(target As UserAccount)
            Dim services = Service.GetByProvider(target.UserId)
            Dim servicePhotoPaths As New List(Of String)
            For Each svc In services
                servicePhotoPaths.AddRange(Service.GetPhotos(svc.ServiceId).ConvertAll(Function(p) p.FilePath))
            Next
            Dim galleryPhotoPaths = ProviderGalleryPhoto.GetByProvider(target.UserId).ConvertAll(Function(p) p.FilePath)

            If Not UserAccount.AdminDeleteUser(target.UserId) Then
                ShowInfo("Не вдалося видалити користувача.")
                Return
            End If

            DeletePhotoFiles(servicePhotoPaths)
            For Each svc In services
                DeleteServiceDirectory(svc.ServiceId)
            Next
            DeletePhotoFiles(galleryPhotoPaths)
            DeleteGalleryDirectory(target.UserId)

            AdminActionLog.Log(CurrentAdmin.UserId, "Видалив користувача",
                String.Format("{0} (оголошень: {1})", target.Email, services.Count))
            ShowInfo("Користувача та його оголошення видалено.")
        End Sub

        Private Sub DeletePhotoFiles(filePaths As List(Of String))
            For Each filePath In filePaths
                Try
                    Dim physicalPath = Server.MapPath(filePath)
                    If File.Exists(physicalPath) Then File.Delete(physicalPath)
                Catch
                End Try
            Next
        End Sub

        Private Sub DeleteServiceDirectory(serviceId As Integer)
            Try
                Dim dir = Server.MapPath("~/Uploads/Services/" & serviceId & "/")
                If Directory.Exists(dir) Then Directory.Delete(dir, True)
            Catch
                ' Директорія може бути непорожня (напр. приховані файли ОС) — не критично.
            End Try
        End Sub

        Private Sub DeleteGalleryDirectory(userId As Integer)
            Try
                Dim dir = Server.MapPath("~/Uploads/Gallery/" & userId & "/")
                If Directory.Exists(dir) Then Directory.Delete(dir, True)
            Catch
            End Try
        End Sub

        Private Sub ShowInfo(message As String)
            infoLabel.Text = message
            infoLabel.Visible = True
        End Sub

    End Class

End Namespace
