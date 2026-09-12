Imports System

Namespace SumyPortal

    Public Class ConfirmEmail
        Inherits System.Web.UI.Page

        Protected Overrides Sub InitializeCulture()
            LocalizationHelper.ApplyCulture(Me)
            MyBase.InitializeCulture()
        End Sub

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            Dim token = Request.QueryString("token")

            If String.IsNullOrEmpty(token) Then
                resultText.Text = Resources.SiteText.ConfirmEmail_NoToken
                Return
            End If

            If UserAccount.ConfirmEmail(token) Then
                resultText.Text = String.Format(Resources.SiteText.ConfirmEmail_Success,
                    "<a href='" & ResolveUrl("~/Login.aspx") & "'>", "</a>")
            Else
                resultText.Text = Resources.SiteText.ConfirmEmail_InvalidToken
            End If
        End Sub

    End Class

End Namespace
