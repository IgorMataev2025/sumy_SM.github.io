Imports System
Imports System.Web.Security

Namespace SumyPortal

    Public Class Login
        Inherits System.Web.UI.Page

        Protected Overrides Sub InitializeCulture()
            LocalizationHelper.ApplyCulture(Me)
            MyBase.InitializeCulture()
        End Sub

        Protected Sub btnLogin_Click(sender As Object, e As EventArgs)
            If Not Page.IsValid Then Return

            Dim email = txtEmail.Text.Trim().ToLowerInvariant()
            Dim errorMessage As String = Nothing
            Dim account = UserAccount.ValidateLogin(email, txtPassword.Text, errorMessage)

            If account Is Nothing Then
                serverErrorLabel.Text = errorMessage
                serverErrorLabel.Visible = True
                Return
            End If

            FormsAuthentication.SetAuthCookie(account.Email, False)
            Response.Redirect(FormsAuthentication.GetRedirectUrl(account.Email, False), False)
        End Sub

    End Class

End Namespace
