Imports System

Namespace SumyPortal

    Public Class ResetPassword
        Inherits System.Web.UI.Page

        Private ReadOnly Property Token As String
            Get
                Return Request.QueryString("token")
            End Get
        End Property

        Protected Overrides Sub InitializeCulture()
            LocalizationHelper.ApplyCulture(Me)
            MyBase.InitializeCulture()
        End Sub

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not IsPostBack AndAlso String.IsNullOrEmpty(Token) Then
                formPanel.Visible = False
                invalidTokenPanel.Visible = True
            End If
        End Sub

        Protected Sub btnSubmit_Click(sender As Object, e As EventArgs)
            If Not Page.IsValid Then Return

            If UserAccount.ResetPassword(Token, txtPassword.Text) Then
                formPanel.Visible = False
                successPanel.Visible = True
            Else
                formPanel.Visible = False
                invalidTokenPanel.Visible = True
            End If
        End Sub

    End Class

End Namespace
