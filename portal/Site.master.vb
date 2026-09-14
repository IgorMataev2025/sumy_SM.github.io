Imports System
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

            Dim isAuthenticated = Page.User.Identity.IsAuthenticated
            anonNav.Visible = Not isAuthenticated
            userNav.Visible = isAuthenticated
            userEmailNav.Visible = isAuthenticated
            If isAuthenticated Then
                userNameLiteral.Text = Server.HtmlEncode(Page.User.Identity.Name)

                Dim account = UserAccount.FindByEmail(Page.User.Identity.Name)
                myServicesLink.Visible = (account IsNot Nothing AndAlso account.UserType = "Provider")

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

    End Class

End Namespace
