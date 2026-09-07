Imports System
Imports MySql.Data.MySqlClient

Namespace SumyPortal

    Public Class RegisterConsumer
        Inherits System.Web.UI.Page

        Protected Sub btnRegister_Click(sender As Object, e As EventArgs)
            If Not Page.IsValid Then Return

            Dim email = txtEmail.Text.Trim().ToLowerInvariant()

            If UserAccount.EmailExists(email) Then
                ShowError("Користувач із таким email вже зареєстрований.")
                Return
            End If

            Try
                Dim token = UserAccount.Register(
                    email, txtPassword.Text, txtFullName.Text.Trim(), txtPhone.Text.Trim(),
                    AccountType.Consumer, False, Nothing, Nothing,
                    txtDistrict.Text.Trim())

                ' Dev-режим (немає SMTP): показуємо посилання прямо на сторінці замість
                ' реального листа. На хостингу тут має бути виклик EmailSender (етап 5/деплой).
                confirmLink.NavigateUrl = ResolveUrl("~/ConfirmEmail.aspx?token=" & Server.UrlEncode(token))
                confirmLink.Text = confirmLink.NavigateUrl

                formPanel.Visible = False
                successPanel.Visible = True
            Catch ex As MySqlException
                ShowError("Не вдалося зберегти дані — спробуйте пізніше.")
            End Try
        End Sub

        Private Sub ShowError(message As String)
            serverErrorLabel.Text = message
            serverErrorLabel.Visible = True
        End Sub

    End Class

End Namespace
