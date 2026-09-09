Imports System

Namespace SumyPortal

    Public Class AdminUsers
        Inherits AdminPageBase

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not IsPostBack Then
                BindUsers()
            End If
        End Sub

        Private Sub BindUsers()
            rptUsers.DataSource = UserAccount.GetAll()
            rptUsers.DataBind()
        End Sub

        Protected Sub rptUsers_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
            If e.CommandName <> "ToggleActive" Then Return

            Dim userId = Convert.ToInt32(e.CommandArgument)
            Dim target = UserAccount.GetAll().Find(Function(u) u.UserId = userId)
            If target Is Nothing Then Return

            If UserAccount.SetActive(userId, Not target.IsActive) Then
                ShowInfo(If(target.IsActive, "Користувача заблоковано.", "Користувача розблоковано."))
                AdminActionLog.Log(CurrentAdmin.UserId, If(target.IsActive, "Заблокував користувача", "Розблокував користувача"), target.Email)
            End If

            BindUsers()
        End Sub

        Private Sub ShowInfo(message As String)
            infoLabel.Text = message
            infoLabel.Visible = True
        End Sub

    End Class

End Namespace
