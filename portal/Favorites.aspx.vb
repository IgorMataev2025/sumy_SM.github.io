Imports System
Imports System.Web.UI.WebControls

Namespace SumyPortal

    ''' <summary>
    ''' Список вподобаних оголошень (п.8 уточненої постановки). Доступний будь-якому
    ''' залогіненому користувачу, захищений стандартним deny users="?" з Web.config
    ''' — окремого <location> не треба (той самий підхід, що MyServices.aspx/Profile.aspx).
    ''' 2026-09-25: таблиця з сортуванням + панель подробиць вибраного рядка (Favorites.aspx).
    ''' </summary>
    Public Class Favorites
        Inherits System.Web.UI.Page

        Protected IsConsumer As Boolean
        Protected CurrentUserId As Integer

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
            IsConsumer = (currentUser.UserType = "Consumer")
            CurrentUserId = currentUser.UserId

            ' Мініатюра, постачальник і рейтинг приходять тим самим запитом (Favorite.GetByUser).
            Dim favorites = Favorite.GetByUser(currentUser.UserId)
            rptRows.DataSource = favorites
            rptRows.DataBind()
            rptDetails.DataSource = favorites
            rptDetails.DataBind()
            tablePanel.Visible = (favorites.Count > 0)
            emptyPanel.Visible = (favorites.Count = 0)
        End Sub

        Protected Function Item(container As RepeaterItem) As Service
            Return CType(container.DataItem, Service)
        End Function

        Protected Function IsAvailable(dataItem As Object) As Boolean
            Return CType(dataItem, Service).Status = "Approved"
        End Function

        Protected Function PriceText(dataItem As Object) As String
            Dim svc = CType(dataItem, Service)
            Return If(svc.Price.HasValue, svc.Price.Value.ToString("0.## грн"), Resources.SiteText.Price_Negotiable)
        End Function

        Protected Function RatingText(dataItem As Object) As String
            Dim svc = CType(dataItem, Service)
            Return If(svc.ReviewAverage.HasValue, "★ " & svc.ReviewAverage.Value.ToString("0.0") & " (" & svc.ReviewCount & ")", "—")
        End Function

        ''' <summary>"Прибрати" в панелі подробиць — Response.Redirect на себе, той самий прийом,
        ''' що btnToggleFavorite_Click у ServiceDetails.aspx.vb. Favorite.Remove фільтрує за
        ''' UserId, тож чужий запис підставленим ServiceId не прибрати.</summary>
        Protected Sub rptDetails_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
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
