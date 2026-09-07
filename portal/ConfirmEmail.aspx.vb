Imports System

Namespace SumyPortal

    Public Class ConfirmEmail
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            Dim token = Request.QueryString("token")

            If String.IsNullOrEmpty(token) Then
                resultText.Text = "Не вказано токен підтвердження."
                Return
            End If

            If UserAccount.ConfirmEmail(token) Then
                resultText.Text = "Email підтверджено. Тепер ви можете <a href='" &
                    ResolveUrl("~/Login.aspx") & "'>увійти</a>."
            Else
                resultText.Text = "Посилання недійсне або вже використане."
            End If
        End Sub

    End Class

End Namespace
