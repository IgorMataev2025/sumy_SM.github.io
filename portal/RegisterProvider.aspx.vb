Imports System
Imports MySql.Data.MySqlClient

Namespace SumyPortal

    Public Class RegisterProvider
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not IsPostBack Then
                legalEntityPanel.Visible = False
            End If
        End Sub

        Protected Sub chkLegalEntity_CheckedChanged(sender As Object, e As EventArgs)
            legalEntityPanel.Visible = chkLegalEntity.Checked
        End Sub

        Protected Sub btnRegister_Click(sender As Object, e As EventArgs)
            If Not Page.IsValid Then Return

            ' Згода на обробку персональних даних (п.4 уточненої постановки) —
            ' CheckBox не покривається RequiredFieldValidator, перевіряємо вручну.
            If Not chkPrivacyConsent.Checked Then
                ShowError("Підтвердьте згоду на обробку персональних даних.")
                Return
            End If

            Dim email = txtEmail.Text.Trim().ToLowerInvariant()
            Dim isLegalEntity = chkLegalEntity.Checked

            ' Серверна валідація полів юр. особи — клієнтські валідатори не спрацюють,
            ' якщо панель була невидима (RegularExpressionValidator в ASP.NET все одно
            ' валідує приховані поля, тому додатково перевіряємо обов'язковість тут).
            If isLegalEntity AndAlso String.IsNullOrWhiteSpace(txtCompanyName.Text) Then
                ShowError("Вкажіть назву компанії.")
                Return
            End If
            If isLegalEntity AndAlso String.IsNullOrWhiteSpace(txtEdrpou.Text) Then
                ShowError("Вкажіть ЄДРПОУ.")
                Return
            End If

            If UserAccount.EmailExists(email) Then
                ShowError("Користувач із таким email вже зареєстрований.")
                Return
            End If

            Try
                Dim token = UserAccount.Register(
                    email, txtPassword.Text, txtFullName.Text.Trim(), txtPhone.Text.Trim(),
                    AccountType.Provider, isLegalEntity,
                    If(isLegalEntity, txtCompanyName.Text.Trim(), Nothing),
                    If(isLegalEntity, txtEdrpou.Text.Trim(), Nothing),
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
