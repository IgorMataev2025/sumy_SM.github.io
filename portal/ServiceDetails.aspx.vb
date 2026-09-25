Imports System
Imports System.Configuration
Imports System.Globalization

Namespace SumyPortal

    Public Class ServiceDetails
        Inherits System.Web.UI.Page

        ''' <summary>Для інлайн-скрипта карти в ServiceDetails.aspx (<%= %>) — culture-invariant,
        ''' щоб десяткова кома uk-UA (globalization Web.config) не зламала JS-літерал числа.</summary>
        Protected Property LatitudeForScript As String
        Protected Property LongitudeForScript As String

        ''' <summary>Alt-текст для фото галереї (розширене SEO, 2026-09-22) — rptPhotos
        ''' прив'язаний до ServicePhoto (без Title), тому назва оголошення виставляється тут
        ''' і читається в розмітці як звичайна властивість сторінки всередині `&lt;%#: %&gt;`
        ''' (databind-вирази виконуються в контексті Page, не лише Container.DataItem).</summary>
        Protected Property GalleryAltText As String

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
            GalleryAltText = svc.Title
            categoryLiteral.Text = Server.HtmlEncode(svc.CategoryName)
            verifiedBadge.Visible = svc.IsVerified

            ' Бейдж "Новинка" (наступна фіча понад MVP, 2026-09-13) — той самий поріг
            ' (Web.config NewServiceDays), що на Catalog.aspx.
            Dim newDays As Integer
            If Not Integer.TryParse(ConfigurationManager.AppSettings("NewServiceDays"), newDays) Then newDays = 7
            newBadge.Visible = (svc.CreatedAt >= DateTime.UtcNow.AddDays(-newDays))
            priceLiteral.Text = If(svc.Price.HasValue, svc.Price.Value.ToString("0.## грн"), Resources.SiteText.Price_Negotiable)
            descriptionLiteral.Text = Server.HtmlEncode(svc.Description)
            providerNameLiteral.Text = Server.HtmlEncode(svc.ProviderName)

            ' Специфікація (наступна фіча понад MVP, реалізовано за прямим запитом користувача,
            ' 2026-09-14) — той самий SpecificationReader, що ServiceEdit.aspx.vb для постачальника.
            If Not String.IsNullOrEmpty(svc.SpecificationFilePath) Then
                Dim specHtml As String = Nothing
                If SpecificationReader.TryReadAsHtmlTable(Server.MapPath(svc.SpecificationFilePath), specHtml) Then
                    specPreviewLiteral.Text = specHtml
                    specPreviewPanel.Visible = True
                End If
            End If

            ' Базове SEO (п.21, наступна фіча понад MVP, 2026-09-12) — унікальний опис
            ' для кожної картки оголошення (найцінніше для SEO, на відміну від
            ' однакового загального опису на решті сторінок); ~155 символів — типова
            ' довжина, яку показує Google в результатах пошуку, довше просто обріже сам.
            Master.MetaDescription = Server.HtmlEncode(If(String.IsNullOrWhiteSpace(svc.Description),
                svc.Title & " — " & svc.CategoryName & " у Сумах та області. Портал послуг Safina.",
                Truncate(svc.Description, 155)))
            Master.OgTitle = Server.HtmlEncode(svc.Title) & " — Портал послуг Safina"
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

            ' Вподобане (п.8 уточненої постановки) — доступне будь-якому залогіненому, крім
            ' власника оголошення (додавати себе в обране безглуздо, 2026-09-25).
            btnToggleFavorite.Visible = (currentUser IsNot Nothing AndAlso currentUser.UserId <> svc.ProviderId)
            If btnToggleFavorite.Visible Then
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

            ' Скарга на оголошення (наступна фіча понад MVP, обрано автономно циклом /loop,
            ' 2026-09-13) — після успішного надсилання Response.Redirect-ить на себе з
            ' query-прапорцем ?reported=1 (той самий принцип, що вподобане/відгук: простіше,
            ' ніж вести два шляхи заповнення полів), тут лише показуємо подяку й ховаємо форму.
            If Request.QueryString("reported") = "1" Then
                reportThanksPanel.Visible = True
                reportFormPanel.Visible = False
            End If

            _isReviewsOwnerView = (currentUser IsNot Nothing AndAlso currentUser.UserId = svc.ProviderId)
            LoadReviews(svc.ServiceId)
            LoadSimilar(svc)
            RenderServiceJsonLd(svc)

            ' Форма відгуку — з 2026-09-25 лише споживач, який уже звертався до постачальника
            ' щодо цього оголошення (CanReview), і лише один раз. Раніше — будь-який
            ' залогінений, крім власника (п.9), що дозволяло відгуки від конкурентів.
            ' Іншому постачальнику (не власнику) нічого не показуємо; споживачу без
            ' звернення — підказку, як отримати право на відгук.
            reviewFormPanel.Visible = False
            alreadyReviewedPanel.Visible = False
            reviewNotAllowedPanel.Visible = False
            If currentUser IsNot Nothing AndAlso currentUser.UserType = "Consumer" Then
                If Review.HasReviewed(svc.ServiceId, currentUser.UserId) Then
                    alreadyReviewedPanel.Visible = True
                ElseIf CanReview(svc, currentUser) Then
                    reviewFormPanel.Visible = True
                Else
                    reviewNotAllowedPanel.Visible = True
                End If
            End If
        End Sub

        ''' <summary>Схожі оголошення (п.24, наступна фіча понад MVP, 2026-09-12) — до 4 інших
        ''' Approved-оголошень тієї ж категорії; блок повністю прихований, якщо немає жодного
        ''' (свідоме MVP-спрощення, без фолбеку на інші категорії/райони).</summary>
        Private Sub LoadSimilar(svc As Service)
            Const SimilarLimit As Integer = 4
            Dim similar = Service.GetSimilar(svc.CategoryId, svc.ServiceId, SimilarLimit)

            For Each item In similar
                Dim photos = Service.GetPhotos(item.ServiceId)
                If photos.Count > 0 Then item.ThumbnailUrl = photos(0).FilePath
            Next

            rptSimilar.DataSource = similar
            rptSimilar.DataBind()
            similarPanel.Visible = (similar.Count > 0)
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

        ''' <summary>JSON-LD Service (розширене SEO/GEO, 2026-09-22) — структуровані дані для
        ''' Google Rich Results і LLM-краулерів. aggregateRating додається лише коли є хоч
        ''' один відгук (схема.org вимагає ratingCount &gt; 0 для валідного aggregateRating),
        ''' offers — лише коли ціна вказана (не "за домовленістю").</summary>
        Private Sub RenderServiceJsonLd(svc As Service)
            Dim baseUrl = Request.Url.GetLeftPart(UriPartial.Authority)
            Dim pageUrl = baseUrl & ResolveUrl("~/ServiceDetails.aspx?id=" & svc.ServiceId)

            Dim json As New System.Text.StringBuilder()
            json.Append("{""@context"":""https://schema.org"",""@type"":""Service"",")
            json.Append("""name"":""").Append(JsonEscape(svc.Title)).Append(""",")
            json.Append("""description"":""").Append(JsonEscape(If(svc.Description, svc.Title))).Append(""",")
            json.Append("""url"":""").Append(JsonEscape(pageUrl)).Append(""",")
            json.Append("""areaServed"":{""@type"":""City"",""name"":""").Append(JsonEscape(If(String.IsNullOrEmpty(svc.District), "Суми", svc.District))).Append("""},")
            json.Append("""serviceType"":""").Append(JsonEscape(svc.CategoryName)).Append(""",")
            json.Append("""provider"":{""@type"":""Organization"",""name"":""").Append(JsonEscape(svc.ProviderName)).Append("""}")

            If svc.Price.HasValue Then
                json.Append(",""offers"":{""@type"":""Offer"",""price"":""").Append(svc.Price.Value.ToString(CultureInfo.InvariantCulture)).Append(""",""priceCurrency"":""UAH""}")
            End If

            Dim average As Decimal? = Nothing
            Dim reviewCount As Integer = 0
            Review.GetSummary(svc.ServiceId, average, reviewCount)
            If reviewCount > 0 AndAlso average.HasValue Then
                json.Append(",""aggregateRating"":{""@type"":""AggregateRating"",""ratingValue"":""").
                    Append(average.Value.ToString("0.0", CultureInfo.InvariantCulture)).
                    Append(""",""reviewCount"":""").Append(reviewCount).Append("""}")
            End If

            json.Append("}")
            serviceJsonLdLiteral.Text = "<script type=""application/ld+json"">" & json.ToString() & "</script>"
        End Sub

        ''' <summary>Мінімальне власне екранування для JSON-значень — той самий прийом, що
        ''' Catalog.aspx.vb/Site.master.vb.</summary>
        Private Function JsonEscape(s As String) As String
            If s Is Nothing Then Return String.Empty
            Return s.Replace("\", "\\").Replace("""", "\""").Replace(vbCr, "").Replace(vbLf, "\n").Replace("</", "<\/")
        End Function

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
        ''' <summary>Право на відгук (2026-09-25): споживач (не постачальник), не власник, і вже
        ''' писав щодо цього оголошення. Та сама перевірка і для показу форми, і на сервері при
        ''' відправці — прихована форма сама по собі не захищає від підробленого POST.</summary>
        Private Shared Function CanReview(svc As Service, user As UserAccount) As Boolean
            Return user.UserType = "Consumer" AndAlso user.UserId <> svc.ProviderId AndAlso
                DialogMessage.HasConversation(svc.ServiceId, user.UserId)
        End Function

        Protected Sub btnSubmitReview_Click(sender As Object, e As EventArgs)
            If Not Page.IsValid Then Return

            Dim serviceId As Integer
            If Not Integer.TryParse(Request.QueryString("id"), serviceId) Then Return

            Dim currentUser = UserAccount.FindByEmail(Page.User.Identity.Name)
            If currentUser Is Nothing Then Return

            Dim svc = Service.GetApprovedById(serviceId)
            If svc Is Nothing OrElse Not CanReview(svc, currentUser) Then Return

            Dim rating As Integer
            If Not Integer.TryParse(ddlRating.SelectedValue, rating) Then Return

            Review.Add(serviceId, currentUser.UserId, rating, txtComment.Text)

            Response.Redirect(Request.RawUrl, True)
        End Sub

        ''' <summary>
        ''' Скарга на оголошення (наступна фіча понад MVP, обрано автономно циклом /loop,
        ''' 2026-09-13) — доступна й анонімам (ReporterEmail лишається Nothing), той самий
        ''' Response.Redirect-на-себе прийом, що btnToggleFavorite_Click/btnSubmitReview_Click.
        ''' Окрема ValidationGroup "ReportForm" (розмітка) — щоб не вимагати заповнення полів
        ''' форми відгуку при відправці цієї форми, і навпаки.
        ''' </summary>
        Protected Sub btnSubmitReport_Click(sender As Object, e As EventArgs)
            Dim serviceId As Integer
            If Not Integer.TryParse(Request.QueryString("id"), serviceId) Then Return

            Dim separator = If(Request.RawUrl.Contains("?"), "&", "?")

            ' Honeypot проти спам-ботів (наступна фіча понад MVP, обрано автономно
            ' циклом /loop, 2026-09-13) — форма анонімна й публічна; вдаємо успіх
            ' (той самий редірект, що й реальна відправка), нічого не зберігаючи.
            If Not String.IsNullOrEmpty(txtReportWebsite.Text) Then
                Response.Redirect(Request.RawUrl & separator & "reported=1", True)
                Return
            End If

            If Not Page.IsValid Then Return

            Dim reason = ddlReportReason.SelectedValue
            If String.IsNullOrWhiteSpace(reason) Then Return

            Dim currentUser = UserAccount.FindByEmail(Page.User.Identity.Name)
            Dim reporterEmail = If(currentUser IsNot Nothing, currentUser.Email, Nothing)

            ServiceReport.Add(serviceId, reporterEmail, reason, txtReportComment.Text)

            Response.Redirect(Request.RawUrl & separator & "reported=1", True)
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
