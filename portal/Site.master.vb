Imports System
Imports System.Globalization
Imports System.IO

Namespace SumyPortal

    Public Class SiteMaster
        Inherits System.Web.UI.MasterPage

        ''' <summary>Базове SEO (п.21, наступна фіча понад MVP, 2026-09-12) — сторінки задають
        ''' свій опис через CType(Master, SiteMaster).MetaDescription = "..." у Page_Load
        ''' (потребує &lt;%@ MasterType VirtualPath="~/Site.master" %&gt; у самій .aspx для
        ''' строгої типізації Master — інакше довелось би кастити щоразу вручну). За
        ''' замовчуванням лишається загальний опис, заданий прямо в Site.master.
        ''' Контрол у розмітці навмисно НЕ metaDescription (та сама грабля, що вже
        ''' Service/AccountType, DialogMessage/Message — README) — VB.NET регістронезалежний
        ''' до імен, тому властивість MetaDescription конфліктувала б із авто-згенерованим
        ''' полем metaDescription для контролу з тим самим Id; контрол перейменовано в
        ''' metaDescriptionTag.</summary>
        Public Property MetaDescription As String
            Get
                Return metaDescriptionTag.Content
            End Get
            Set(value As String)
                metaDescriptionTag.Content = value
            End Set
        End Property

        ''' <summary>Розширене SEO/GEO (2026-09-22) — той самий патерн, що MetaDescription
        ''' вище: сторінка задає свій og:title/twitter:title через
        ''' CType(Master, SiteMaster).OgTitle = "..." у власному Page_Load (виконується ДО
        ''' Page_Load master-сторінки — Load дочірніх контролів, включно з MasterPage,
        ''' спрацьовує вже після Load самої Page — тому значення встигає застосуватись до
        ''' фінального рендеру без додаткової синхронізації). За замовчуванням лишається
        ''' назва порталу, задана прямо в розмітці.</summary>
        Public Property OgTitle As String
            Get
                Return ogTitleTag.Content
            End Get
            Set(value As String)
                ogTitleTag.Content = value
                twitterTitleTag.Content = value
            End Set
        End Property

        ''' <summary>Кеш-бастинг для site.css (2026-09-14) — IIS роздає статику з
        ''' Cache-Control: max-age=31536000 (рік), тому вже відкриті у відвідувача
        ''' версії CSS/JS кешуються браузером надовго; кожен наступний CSS-фікс без
        ''' цього був би невидимий для того самого відвідувача аж до explicit
        ''' Ctrl+F5. Час останньої зміни файлу як query-параметр змінює URL і
        ''' форсує повторне завантаження автоматично — без ручного номера версії,
        ''' який довелось би не забувати піднімати при кожному деплої CSS.</summary>
        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            Dim cssVersion As String = "0"
            Try
                cssVersion = File.GetLastWriteTimeUtc(Server.MapPath("~/css/site.css")).Ticks.ToString()
            Catch
            End Try
            cssLink.Href = ResolveUrl("~/css/site.css") & "?v=" & cssVersion

            ' Розширене SEO/GEO (2026-09-22) — canonical/og:url завжди самопосилання на
            ' поточний абсолютний запит (домен береться з Request.Url, той самий принцип,
            ' що вже Sitemap.vb — коректно і на *.etempurl.com, і після купівлі домену без
            ' змін коду). og:description/twitter:description дзеркалять metaDescriptionTag —
            ' на момент виконання цього Page_Load (дочірній контрол MasterPage) власний
            ' Page_Load контентної сторінки вже відпрацював і встиг застосувати свій
            ' override через властивість MetaDescription вище.
            Dim currentUrl = Request.Url.AbsoluteUri
            canonicalTag.HRef = currentUrl
            ogUrlTag.Content = currentUrl
            ogDescriptionTag.Content = metaDescriptionTag.Content
            twitterDescriptionTag.Content = metaDescriptionTag.Content
            ogLocaleTag.Content = If(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName = LocalizationHelper.EnglishCulture,
                "en_US", "uk_UA")

            ' JSON-LD Organization/WebSite (GEO) — статичний опис порталу, домен з запиту.
            Dim baseUrl = Request.Url.GetLeftPart(UriPartial.Authority)
            Dim orgJson = "{""@context"":""https://schema.org"",""@type"":""Organization"",""name"":""Портал послуг Safina""," &
                """url"":""" & JsonEscape(baseUrl) & "/""," &
                """description"":""Портал послуг для мешканців Сум та області""," &
                """areaServed"":{""@type"":""City"",""name"":""Суми""}}"
            orgJsonLdLiteral.Text = "<script type=""application/ld+json"">" & orgJson & "</script>"

            Dim isAuthenticated = Page.User.Identity.IsAuthenticated
            anonNav.Visible = Not isAuthenticated
            userNav.Visible = isAuthenticated
            userEmailNav.Visible = isAuthenticated
            ' Гостю Каталог/Стіл замовлень у меню не показуються (2026-09-24): дублюють
            ' одне одного й ускладнюють перший контакт із сайтом. Гість потрапляє в каталог
            ' через картки категорій на головній; самі сторінки лишаються відкритими
            ' анонімно (Web.config, SEO/sitemap).
            catalogNavLink.Visible = isAuthenticated
            orderBoardNavLink.Visible = isAuthenticated
            If isAuthenticated Then
                userNameLiteral.Text = Server.HtmlEncode(Page.User.Identity.Name)

                Dim account = UserAccount.FindByEmail(Page.User.Identity.Name)
                myServicesLink.Visible = (account IsNot Nothing AndAlso account.UserType = "Provider")

                If account IsNot Nothing Then
                    userRoleBadge.Visible = True
                    userRoleBadge.InnerText = If(account.UserType = "Provider",
                        Resources.SiteText.UserRole_Provider, Resources.SiteText.UserRole_Consumer)

                    ' Постачальнику "Головна" (лендинг для нових відвідувачів) не потрібна —
                    ' Каталог/Стіл замовлень лишаються (2026-09-22).
                    homeNavLink.Visible = (account.UserType <> "Provider")
                End If

                ' Лічильник непрочитаних повідомлень (наступна фіча понад MVP, обрано
                ' автономно циклом /loop, 2026-09-13) — один легкий COUNT-запит на
                ' кожне завантаження сторінки залогіненим користувачем (індексовано,
                ' той самий принцип, що вже AdminDashboard.aspx рахує лічильники плиток).
                If account IsNot Nothing Then
                    Dim unreadCount = DialogMessage.GetUnreadCountForUser(account.UserId)
                    If unreadCount > 0 Then
                        navUnreadBadge.Visible = True
                        navUnreadBadge.InnerText = unreadCount.ToString()
                    End If
                End If
            End If
        End Sub

        ''' <summary>Мінімальне власне екранування для JSON-значень у &lt;script
        ''' type="application/ld+json"&gt; — той самий прийом (і той самий код), що вже
        ''' Catalog.aspx.vb: JSON.stringify тут немає, а "&lt;/" екранується окремо, бо
        ''' браузер закриває &lt;script&gt; за буквальним "&lt;/script" незалежно від значення type.</summary>
        Private Function JsonEscape(s As String) As String
            If s Is Nothing Then Return String.Empty
            Return s.Replace("\", "\\").Replace("""", "\""").Replace(vbCr, "").Replace(vbLf, "\n").Replace("</", "<\/")
        End Function

    End Class

End Namespace
