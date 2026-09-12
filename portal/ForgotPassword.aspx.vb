Imports System

Namespace SumyPortal

    Public Class ForgotPassword
        Inherits System.Web.UI.Page

        Protected Overrides Sub InitializeCulture()
            LocalizationHelper.ApplyCulture(Me)
            MyBase.InitializeCulture()
        End Sub

        Protected Sub btnSubmit_Click(sender As Object, e As EventArgs)
            If Not Page.IsValid Then Return

            Dim email = txtEmail.Text.Trim().ToLowerInvariant()
            Dim token = UserAccount.CreatePasswordResetToken(email)

            ' Навмисно НЕ повідомляємо, чи існує email — однакове повідомлення в обох
            ' випадках, щоб форма не використовувалась для перевірки зареєстрованих адрес.
            If Not String.IsNullOrEmpty(token) Then
                resetLink.NavigateUrl = ResolveUrl("~/ResetPassword.aspx?token=" & Server.UrlEncode(token))
                resetLink.Text = resetLink.NavigateUrl
                resetLink.Visible = True
            Else
                resetLink.Visible = False
            End If

            formPanel.Visible = False
            resultPanel.Visible = True
        End Sub

    End Class

End Namespace
