Imports System

Namespace SumyPortal

    ''' <summary>
    ''' Список вподобаних оголошень (п.8 уточненої постановки). Доступний будь-якому
    ''' залогіненому користувачу, захищений стандартним deny users="?" з Web.config
    ''' — окремого <location> не треба (той самий підхід, що MyServices.aspx/Profile.aspx).
    ''' </summary>
    Public Class Favorites
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If IsPostBack Then Return

            Dim currentUser = UserAccount.FindByEmail(Page.User.Identity.Name)
            If currentUser Is Nothing Then
                Response.Redirect("~/Default.aspx", True)
                Return
            End If

            Dim favorites = Favorite.GetByUser(currentUser.UserId)
            rptFavorites.DataSource = favorites
            rptFavorites.DataBind()
            emptyPanel.Visible = (favorites.Count = 0)
        End Sub

    End Class

End Namespace
