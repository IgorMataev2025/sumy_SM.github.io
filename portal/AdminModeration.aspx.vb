Imports System
Imports System.Web.UI.WebControls

Namespace SumyPortal

    Public Class AdminModeration
        Inherits AdminPageBase

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not IsPostBack Then
                BindQueue()
            End If
        End Sub

        Private Sub BindQueue()
            Dim queue = Service.GetPendingForModeration()
            rptQueue.DataSource = queue
            rptQueue.DataBind()
            emptyPanel.Visible = (queue.Count = 0)
        End Sub

        Protected Sub rptQueue_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
            Dim serviceId = Convert.ToInt32(e.CommandArgument)

            Select Case e.CommandName
                Case "Approve"
                    If Service.Approve(serviceId, CurrentAdmin.UserId) Then
                        ShowInfo("Оголошення опубліковано.")
                    End If

                Case "Reject"
                    Dim txtReason = TryCast(e.Item.FindControl("txtRejectReason"), TextBox)
                    Dim reason = If(txtReason IsNot Nothing, txtReason.Text.Trim(), String.Empty)

                    If String.IsNullOrEmpty(reason) Then
                        ShowInfo("Для відхилення потрібно вказати причину.")
                    ElseIf Service.Reject(serviceId, CurrentAdmin.UserId, reason) Then
                        ShowInfo("Оголошення відхилено.")
                    End If
            End Select

            BindQueue()
        End Sub

        Private Sub ShowInfo(message As String)
            infoLabel.Text = message
            infoLabel.Visible = True
        End Sub

    End Class

End Namespace
