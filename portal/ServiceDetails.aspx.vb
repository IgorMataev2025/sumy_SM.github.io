Imports System

Namespace SumyPortal

    Public Class ServiceDetails
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If IsPostBack Then Return

            Dim serviceId As Integer
            If Not Integer.TryParse(Request.QueryString("id"), serviceId) Then
                ShowNotFound()
                Return
            End If

            Dim svc = Service.GetApprovedById(serviceId)
            If svc Is Nothing Then
                ShowNotFound()
                Return
            End If

            titleLiteral.Text = svc.Title
            headingLiteral.Text = svc.Title
            categoryLiteral.Text = svc.CategoryName
            priceLiteral.Text = If(svc.Price.HasValue, svc.Price.Value.ToString("0.## грн"), "Ціна за домовленістю")
            descriptionLiteral.Text = svc.Description
            providerNameLiteral.Text = svc.ProviderName
            phoneLiteral.Text = svc.Phone
            districtLiteral.Text = If(String.IsNullOrEmpty(svc.District), "не вказано", svc.District)

            ' Відкритий перегляд для USER (п.12 уточненої постановки, 2026-09-11):
            ' сторінка тепер доступна анонімно (Web.config), але телефон —
            ' реєстраційний реквізит постачальника — ховаємо від неавторизованих.
            Dim isAuthenticated = Page.User.Identity.IsAuthenticated
            phoneHolder.Visible = isAuthenticated
            anonContactPanel.Visible = Not isAuthenticated

            Dim photos = Service.GetPhotos(svc.ServiceId)
            rptPhotos.DataSource = photos
            rptPhotos.DataBind()

            ' Договір формується для замовника, а не для власника оголошення (уточнена
            ' постановка від 2026-09-07, п.3) — посилання ховаємо, якщо дивиться сам постачальник.
            Dim currentUser = UserAccount.FindByEmail(Page.User.Identity.Name)
            contractLink.Visible = (currentUser IsNot Nothing AndAlso currentUser.UserId <> svc.ProviderId)
            contractLink.NavigateUrl = ResolveUrl("~/ServiceContract.aspx?id=" & svc.ServiceId)

            ' Вподобане (п.8 уточненої постановки) — доступне будь-якому залогіненому.
            btnToggleFavorite.Visible = (currentUser IsNot Nothing)
            If currentUser IsNot Nothing Then
                Dim isFavorite = Favorite.IsFavorite(currentUser.UserId, svc.ServiceId)
                btnToggleFavorite.Text = If(isFavorite, "★ Прибрати з обраного", "☆ Додати в обране")
            End If

            ' Повідомлення постачальнику (п.10 уточненої постановки) — MVP-спрощення:
            ' ініціює лише споживач (не власник, і не інший постачальник, що переглядає
            ' чужу картку), постачальник далі лише відповідає в уже створеній розмові.
            messageLink.Visible = (currentUser IsNot Nothing AndAlso currentUser.UserType = "Consumer")
            If currentUser IsNot Nothing Then
                messageLink.NavigateUrl = ResolveUrl("~/MessageThread.aspx?serviceId=" & svc.ServiceId & "&consumerId=" & currentUser.UserId)
            End If

            LoadReviews(svc.ServiceId)

            ' Форма відгуку — будь-який залогінений, крім самого власника оголошення
            ' (п.9 уточненої постановки), і лише якщо ще не залишав відгук на нього.
            If currentUser Is Nothing OrElse currentUser.UserId = svc.ProviderId Then
                reviewFormPanel.Visible = False
                alreadyReviewedPanel.Visible = False
            ElseIf Review.HasReviewed(svc.ServiceId, currentUser.UserId) Then
                reviewFormPanel.Visible = False
                alreadyReviewedPanel.Visible = True
            Else
                reviewFormPanel.Visible = True
                alreadyReviewedPanel.Visible = False
            End If
        End Sub

        Private Sub LoadReviews(serviceId As Integer)
            Dim reviews = Review.GetByService(serviceId)
            rptReviews.DataSource = reviews
            rptReviews.DataBind()
            noReviewsPanel.Visible = (reviews.Count = 0)

            Dim average As Decimal? = Nothing
            Dim count As Integer = 0
            Review.GetSummary(serviceId, average, count)
            ratingSummaryLiteral.Text = If(average.HasValue,
                String.Format("★ {0:0.0} ({1} {2})", average.Value, count, PluralizeReviews(count)),
                "Ще немає відгуків")
        End Sub

        ''' <summary>Українська відміна іменника "відгук" за кількістю (1/2-4/5+, з винятком 11-14).</summary>
        Private Shared Function PluralizeReviews(count As Integer) As String
            Dim mod100 = count Mod 100
            If mod100 >= 11 AndAlso mod100 <= 14 Then Return "відгуків"
            Select Case count Mod 10
                Case 1 : Return "відгук"
                Case 2, 3, 4 : Return "відгуки"
                Case Else : Return "відгуків"
            End Select
        End Function

        ''' <summary>
        ''' Той самий прийом, що btnToggleFavorite_Click, — Response.Redirect на себе
        ''' після збереження замість ведення двох шляхів заповнення полів.
        ''' </summary>
        Protected Sub btnSubmitReview_Click(sender As Object, e As EventArgs)
            If Not Page.IsValid Then Return

            Dim serviceId As Integer
            If Not Integer.TryParse(Request.QueryString("id"), serviceId) Then Return

            Dim currentUser = UserAccount.FindByEmail(Page.User.Identity.Name)
            If currentUser Is Nothing Then Return

            Dim svc = Service.GetApprovedById(serviceId)
            If svc Is Nothing OrElse currentUser.UserId = svc.ProviderId Then Return

            Dim rating As Integer
            If Not Integer.TryParse(ddlRating.SelectedValue, rating) Then Return

            Review.Add(serviceId, currentUser.UserId, rating, txtComment.Text)

            Response.Redirect(Request.RawUrl, True)
        End Sub

        ''' <summary>
        ''' Перемикає вподобане й перезавантажує сторінку звичайним GET
        ''' (Response.Redirect на себе) — простіше, ніж вести два шляхи заповнення
        ''' полів (початкове завантаження й постбек), той самий підхід, що вже
        ''' використовує SaveService у ServiceEdit.aspx.vb для чернетки.
        ''' </summary>
        Protected Sub btnToggleFavorite_Click(sender As Object, e As EventArgs)
            Dim serviceId As Integer
            If Not Integer.TryParse(Request.QueryString("id"), serviceId) Then Return

            Dim currentUser = UserAccount.FindByEmail(Page.User.Identity.Name)
            If currentUser Is Nothing Then Return

            If Favorite.IsFavorite(currentUser.UserId, serviceId) Then
                Favorite.Remove(currentUser.UserId, serviceId)
            Else
                Favorite.Add(currentUser.UserId, serviceId)
            End If

            Response.Redirect(Request.RawUrl, True)
        End Sub

        Private Sub ShowNotFound()
            detailsPanel.Visible = False
            notFoundPanel.Visible = True
        End Sub

    End Class

End Namespace
