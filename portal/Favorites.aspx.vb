Imports System
Imports System.Web.UI.WebControls

Namespace SumyPortal

    ''' <summary>
    ''' Список вподобаних оголошень (п.8 уточненої постановки). Доступний будь-якому
    ''' залогіненому користувачу, захищений стандартним deny users="?" з Web.config
    ''' — окремого <location> не треба (той самий підхід, що MyServices.aspx/Profile.aspx).
    ''' </summary>
    Public Class Favorites
        Inherits System.Web.UI.Page

        Protected Overrides Sub InitializeCulture()
            LocalizationHelper.ApplyCulture(Me)
            MyBase.InitializeCulture()
        End Sub

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If IsPostBack Then Return

            Dim currentUser = UserAccount.FindByEmail(Page.User.Identity.Name)
            If currentUser Is Nothing Then
                Response.Redirect("~/Default.aspx", True)
                Return
            End If

            Dim favorites = Favorite.GetByUser(currentUser.UserId)
            ' Мініатюра — перше фото, той самий прийом, що Catalog.aspx.vb.
            For Each item In favorites
                Dim photos = Service.GetPhotos(item.ServiceId)
                If photos.Count > 0 Then item.ThumbnailUrl = photos(0).FilePath
            Next
            rptFavorites.DataSource = favorites
            rptFavorites.DataBind()
            emptyPanel.Visible = (favorites.Count = 0)
        End Sub

        Protected Function IsAvailable(dataItem As Object) As Boolean
            Return CType(dataItem, Service).Status = "Approved"
        End Function

        ''' <summary>"Прибрати" на картці — Response.Redirect на себе, той самий прийом, що
        ''' btnToggleFavorite_Click у ServiceDetails.aspx.vb. Favorite.Remove фільтрує за
        ''' UserId, тож чужий запис підставленим ServiceId не прибрати.</summary>
        Protected Sub rptFavorites_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
            If e.CommandName <> "Remove" Then Return
            Dim serviceId As Integer
            If Not Integer.TryParse(CStr(e.CommandArgument), serviceId) Then Return

            Dim currentUser = UserAccount.FindByEmail(Page.User.Identity.Name)
            If currentUser Is Nothing Then Return

            Favorite.Remove(currentUser.UserId, serviceId)
            Response.Redirect(Request.RawUrl, True)
        End Sub

    End Class

End Namespace
