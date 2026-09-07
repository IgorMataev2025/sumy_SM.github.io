Imports System

Namespace SumyPortal

    Public Class SiteMaster
        Inherits System.Web.UI.MasterPage

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            Dim isAuthenticated = Page.User.Identity.IsAuthenticated
            anonNav.Visible = Not isAuthenticated
            userNav.Visible = isAuthenticated
            If isAuthenticated Then
                userNameLiteral.Text = Server.HtmlEncode(Page.User.Identity.Name)

                Dim account = UserAccount.FindByEmail(Page.User.Identity.Name)
                myServicesLink.Visible = (account IsNot Nothing AndAlso account.UserType = "Provider")
                adminNav.Visible = (account IsNot Nothing AndAlso account.IsAdmin)
            End If
        End Sub

    End Class

End Namespace
