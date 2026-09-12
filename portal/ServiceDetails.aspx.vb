Imports System
Imports System.Globalization

Namespace SumyPortal

    Public Class ServiceDetails
        Inherits System.Web.UI.Page

        ''' <summary>Для інлайн-скрипта карти в ServiceDetails.aspx (<%= %>) — culture-invariant,
        ''' щоб десяткова кома uk-UA (globalization Web.config) не зламала JS-літерал числа.</summary>
        Protected Property LatitudeForScript As String
        Protected Property LongitudeForScript As String

        ''' <summary>Відповідь постачальника на відгук (п.19, наступна фіча понад MVP, 2026-09-12) —
        ''' чи показувати форму відповіді під кожним відгуком у rptReviews_ItemDataBound.
        ''' Обчислюється в LoadReviews лише при не-постбек завантаженні (той самий виклик,
        ''' що й сам LoadReviews) — постбек-обробник rptReviews_ItemCommand цим полем НЕ
        ''' користується для перевірки прав, лише для UI: справжній захист власника — у
        ''' самому SQL Review.SetProviderReply (JOIN на Services.ProviderId).</summary>
        Private _isReviewsOwnerView As Boolean

        ''' <summary>Багатомовність (постановка робочої тестової версії, 2026-09-12) —
        ''' офіційна точка ASP.NET Web Forms для програмної культури, до того як
        ''' вона "застигне" на задекларованому в Web.config значенні (uk-UA).</summary>
        Protected Overrides Sub InitializeCulture()
            LocalizationHelper.ApplyCulture(Me)
            MyBase.InitializeCulture()
        End Sub

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

            ' Статистика для постачальника (п.17, наступна фіча понад MVP, 2026-09-12) —
            ' +1 до лічильника переглядів при кожному не-постбек завантаженні (IsPostBack
            ' перевірено на самому початку Page_Load вище); власні перегляди постачальника
            ' теж рахуються, свідомо не фільтруємо.
            Service.IncrementViewCount(svc.ServiceId)

            ' Server.HtmlEncode — сторінка публічна (доступна анонімам, Web.config),
            ' а Title/Description/ProviderName/Phone — вільний текст постачальника
            ' (той самий прийом, що вже в ServiceContract.aspx.vb).
            titleLiteral.Text = Server.HtmlEncode(svc.Title)
            headingLiteral.Text = Server.HtmlEncode(svc.Title)
            categoryLiteral.Text = Server.HtmlEncode(svc.CategoryName)
            priceLiteral.Text = If(svc.Price.HasValue, svc.Price.Value.ToString("0.## грн"), Resources.SiteText.Price_Negotiable)
            descriptionLiteral.Text = Server.HtmlEncode(svc.Description)
            providerNameLiteral.Text = Server.HtmlEncode(svc.ProviderName)

            ' Базове SEO (п.21, наступна фіча понад MVP, 2026-09-12) — унікальний опис
            ' для кожної картки оголошення (найцінніше для SEO, на відміну від
            ' однакового загального опису на решті сторінок); ~155 символів — типова
            ' довжина, яку показує Google в результатах пошуку, довше просто обріже сам.
            Master.MetaDescription = Server.HtmlEncode(If(String.IsNullOrWhiteSpace(svc.Description),
                svc.Title & " — " & svc.CategoryName & " у Сумах та області. Портал послуг Safina.",
                Truncate(svc.Description, 155)))
            ' Публічна галерея на акаунті постачальника (Profile.aspx?providerId=X) —
            ' доступна будь-кому, включно з анонімами (постановка робочої тестової версії, 2026-09-12).
            providerGalleryLink.NavigateUrl = ResolveUrl("~/Profile.aspx?providerId=" & svc.ProviderId)
            phoneLiteral.Text = Server.HtmlEncode(svc.Phone)
            districtLiteral.Text = Server.HtmlEncode(If(String.IsNullOrEmpty(svc.District), Resources.SiteText.Details_NotSpecified, svc.District))

            ' Геолокація (постановка робочої тестової версії, 2026-09-12) — лише інформативна
            ' мітка, не замінює District. Панель/скрипт рендеряться лише коли координати є.
            If svc.Latitude.HasValue AndAlso svc.Longitude.HasValue Then
                LatitudeForScript = svc.Latitude.Value.ToString(CultureInfo.InvariantCulture)
                LongitudeForScript = svc.Longitude.Value.ToString(CultureInfo.InvariantCulture)
                mapPanel.Visible = True
            End If

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
                btnToggleFavorite.Text = If(isFavorite, Resources.SiteText.Details_Favorite_Remove, Resources.SiteText.Details_Favorite_Add)
            End If

            ' Повідомлення постачальнику (п.10 уточненої постановки) — MVP-спрощення:
            ' ініціює лише споживач (не власник, і не інший постачальник, що переглядає
            ' чужу картку), постачальник далі лише відповідає в уже створеній розмові.
            messageLink.Visible = (currentUser IsNot Nothing AndAlso currentUser.UserType = "Consumer")
            If currentUser IsNot Nothing Then
                messageLink.NavigateUrl = ResolveUrl("~/MessageThread.aspx?serviceId=" & svc.ServiceId & "&consumerId=" & currentUser.UserId)
            End If

            _isReviewsOwnerView = (currentUser IsNot Nothing AndAlso currentUser.UserId = svc.ProviderId)
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
                Resources.SiteText.Reviews_Summary_None)
        End Sub

        ''' <summary>Показує готову відповідь постачальника (якщо є) і форму для її
        ''' додавання/редагування (лише власнику оголошення — п.19, наступна фіча понад
        ''' MVP, 2026-09-12).</summary>
        Protected Sub rptReviews_ItemDataBound(sender As Object, e As RepeaterItemEventArgs)
            If e.Item.ItemType <> ListItemType.Item AndAlso e.Item.ItemType <> ListItemType.AlternatingItem Then Return

            Dim review = CType(e.Item.DataItem, Review)

            If Not String.IsNullOrEmpty(review.ProviderReply) Then
                Dim providerReplyPanel = CType(e.Item.FindControl("providerReplyPanel"), Panel)
                Dim providerReplyLiteral = CType(e.Item.FindControl("providerReplyLiteral"), Literal)
                providerReplyPanel.Visible = True
                providerReplyLiteral.Text = Server.HtmlEncode(review.ProviderReply)
            End If

            Dim ownerReplyFormPanel = CType(e.Item.FindControl("ownerReplyFormPanel"), Panel)
            ownerReplyFormPanel.Visible = _isReviewsOwnerView
        End Sub

        ''' <summary>Постачальник зберігає/редагує/прибирає відповідь під конкретним відгуком.
        ''' Не покладається на _isReviewsOwnerView (те поле лише для UI й не заповнюється на
        ''' постбеку — Page_Load виходить на самому початку при IsPostBack) — справжній захист
        ''' від чужого відгуку в самому Review.SetProviderReply (SQL JOIN на ProviderId).</summary>
        Protected Sub rptReviews_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
            If e.CommandName <> "Reply" Then Return

            Dim currentUser = UserAccount.FindByEmail(Page.User.Identity.Name)
            If currentUser Is Nothing Then Return

            Dim reviewId As Integer
            If Not Integer.TryParse(Convert.ToString(e.CommandArgument), reviewId) Then Return

            Dim txtReply = TryCast(e.Item.FindControl("txtProviderReply"), TextBox)
            If txtReply Is Nothing Then Return

            Review.SetProviderReply(reviewId, currentUser.UserId, txtReply.Text)

            Response.Redirect(Request.RawUrl, True)
        End Sub

        ''' <summary>Обрізає текст до заданої довжини на межі слова (не посеред слова) —
        ''' для meta description (п.21, наступна фіча понад MVP, 2026-09-12). Якщо текст і
        ''' так коротший — повертає без змін, без "…" в кінці.</summary>
        Private Function Truncate(text As String, maxLength As Integer) As String
            If text.Length <= maxLength Then Return text
            Dim cut = text.Substring(0, maxLength)
            Dim lastSpace = cut.LastIndexOf(" "c)
            If lastSpace > 0 Then cut = cut.Substring(0, lastSpace)
            Return cut.TrimEnd() & "…"
        End Function

        ''' <summary>Відміна іменника "відгук" за кількістю. Українська: 1/2-4/5+ з винятком
        ''' 11-14 (гл.63 ЦК тут ні до чого, це просто мова). Англійська (багатомовність,
        ''' постановка робочої тестової версії, 2026-09-12): проста однина/множина.</summary>
        Private Function PluralizeReviews(count As Integer) As String
            ' Не порівнювати рядок UICulture напряму з "en" — сетер Page.UICulture
            ' нормалізує нейтральну культуру "en" у конкретну "en-US" (CultureInfo.
            ' CreateSpecificCulture), тому TwoLetterISOLanguageName надійніший
            ' (знайдено живим тестом — "1 review" показувало українське "відгук").
            If CultureInfo.CurrentUICulture.TwoLetterISOLanguageName = LocalizationHelper.EnglishCulture Then
                Return If(count = 1, Resources.SiteText.Review_Singular, Resources.SiteText.Review_Plural)
            End If

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
