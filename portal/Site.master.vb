Imports System

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

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            Dim isAuthenticated = Page.User.Identity.IsAuthenticated
            anonNav.Visible = Not isAuthenticated
            userNav.Visible = isAuthenticated
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
