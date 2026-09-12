Imports System
Imports MySql.Data.MySqlClient

Namespace SumyPortal

    Public Class RegisterConsumer
        Inherits System.Web.UI.Page

        Protected Overrides Sub InitializeCulture()
            LocalizationHelper.ApplyCulture(Me)
            MyBase.InitializeCulture()
        End Sub

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not IsPostBack Then
                SumyDistricts.Populate(ddlDistrict, Resources.SiteText.District_NotSpecified)
            End If
        End Sub

        Protected Sub btnRegister_Click(sender As Object, e As EventArgs)
            If Not Page.IsValid Then Return

            ' Згода на обробку персональних даних (п.4 уточненої постановки) —
            ' CheckBox не покривається RequiredFieldValidator, перевіряємо вручну.
            If Not chkPrivacyConsent.Checked Then
                ShowError(Resources.SiteText.Register_Err_Consent)
                Return
            End If

            Dim email = txtEmail.Text.Trim().ToLowerInvariant()

            If UserAccount.EmailExists(email) Then
                ShowError(Resources.SiteText.Register_Err_EmailExists)
                Return
            End If

            Try
                Dim token = UserAccount.Register(
                    email, txtPassword.Text, txtFullName.Text.Trim(), txtPhone.Text.Trim(),
                    AccountType.Consumer, False, Nothing, Nothing,
                    ddlDistrict.SelectedValue)

                ' Dev-режим (немає SMTP): показуємо посилання прямо на сторінці замість
                ' реального листа. На хостингу тут має бути виклик EmailSender (етап 5/деплой).
                confirmLink.NavigateUrl = ResolveUrl("~/ConfirmEmail.aspx?token=" & Server.UrlEncode(token))
                confirmLink.Text = confirmLink.NavigateUrl

                formPanel.Visible = False
                successPanel.Visible = True
            Catch ex As MySqlException
                ShowError(Resources.SiteText.Register_Err_SaveFailed)
            End Try
        End Sub

        Private Sub ShowError(message As String)
            serverErrorLabel.Text = message
            serverErrorLabel.Visible = True
        End Sub

    End Class

End Namespace
