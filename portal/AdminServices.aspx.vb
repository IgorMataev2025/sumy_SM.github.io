Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Web.UI.WebControls

Namespace SumyPortal

    ''' <summary>
    ''' Прямий CRUD адміна над усіма оголошеннями (п.12 уточненої постановки,
    ''' 2026-09-11) — на відміну від AdminModeration.aspx (черга Pending,
    ''' approve/reject) тут видно оголошення в БУДЬ-ЯКОМУ статусі, і адмін
    ''' може редагувати чи видаляти будь-яке з них напряму.
    ''' </summary>
    Public Class AdminServices
        Inherits AdminPageBase

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not IsPostBack Then
                txtSearch.Text = QueryParam("q")
                If ddlStatusFilter.Items.FindByValue(QueryParam("status")) IsNot Nothing Then ddlStatusFilter.SelectedValue = QueryParam("status")
                BindServices()
            End If
        End Sub

        ''' <summary>Пошук + статус + сторінки на рівні БД (2026-09-25, аудит Адміна, п.5) —
        ''' раніше всі оголошення вантажились у пам'ять і фільтрувались лише за статусом.</summary>
        Private Sub BindServices()
            Dim total As Integer
            Dim items = Service.AdminSearch(QueryParam("q"), QueryParam("status"), CurrentPageNumber, AdminPageSize, total)
            rptServices.DataSource = items
            rptServices.DataBind()
            emptyPanel.Visible = (items.Count = 0)
            BindPager(lnkPrev, lnkNext, pageInfoLabel, total,
                      New Dictionary(Of String, String) From {{"q", QueryParam("q")}, {"status", QueryParam("status")}})
        End Sub

        Protected Sub btnSearch_Click(sender As Object, e As EventArgs)
            RedirectWithParams(New Dictionary(Of String, String) From {{"q", txtSearch.Text}, {"status", ddlStatusFilter.SelectedValue}})
        End Sub

        Protected Sub rptServices_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
            If e.CommandName = "Delete" Then
                Dim serviceId = Convert.ToInt32(e.CommandArgument)
                Dim svc = Service.GetByIdAny(serviceId)

                If svc IsNot Nothing Then
                    ' Знімаємо шляхи до фото ДО видалення рядка — ServicePhotos
                    ' видаляється каскадно разом із Services (ON DELETE CASCADE).
                    Dim photos = Service.GetPhotos(serviceId)

                    If Service.AdminDelete(serviceId) Then
                        DeletePhotoFiles(photos, serviceId)
                        AdminActionLog.Log(CurrentAdmin.UserId, "Видалив оголошення",
                            String.Format("""{0}"" (постачальник {1})", svc.Title, svc.ProviderEmail))
                        ShowInfo("Оголошення видалено.")
                    End If
                End If
            End If

            BindServices()
        End Sub

        ''' <summary>Той самий підхід, що ServiceEdit.aspx.vb: помилка видалення файлу з диска
        ''' не критична (запис у БД уже прибрано), тому в Try/Catch.</summary>
        Private Sub DeletePhotoFiles(photos As List(Of ServicePhoto), serviceId As Integer)
            For Each photo In photos
                Try
                    Dim physicalPath = Server.MapPath(photo.FilePath)
                    If File.Exists(physicalPath) Then File.Delete(physicalPath)
                Catch
                End Try
            Next

            Try
                Dim dir = Server.MapPath("~/Uploads/Services/" & serviceId & "/")
                If Directory.Exists(dir) Then Directory.Delete(dir)
            Catch
                ' Директорія може бути непорожня (напр. приховані файли ОС) — не критично.
            End Try
        End Sub

        Private Sub ShowInfo(message As String)
            infoLabel.Text = message
            infoLabel.Visible = True
        End Sub

    End Class

End Namespace
