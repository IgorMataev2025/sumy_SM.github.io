Imports System
Imports System.Web.UI.WebControls

Namespace SumyPortal

    ''' <summary>
    ''' Черга відкритих скарг на оголошення (наступна фіча понад MVP, обрано
    ''' автономно циклом /loop, 2026-09-13) — сигнал адміну від будь-кого
    ''' (включно з анонімом), не автоматична модерація. Той самий FIFO-принцип,
    ''' що AdminModeration.aspx (черга Pending), але без approve/reject —
    ''' лише "позначити переглянутою" (дію над самим оголошенням адмін виконує
    ''' окремо на AdminServices.aspx/AdminServiceEdit.aspx).
    ''' </summary>
    Public Class AdminReports
        Inherits AdminPageBase

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not IsPostBack Then
                BindReports()
            End If
        End Sub

        Private Sub BindReports()
            Dim reports = ServiceReport.GetOpen()
            rptReports.DataSource = reports
            rptReports.DataBind()
            emptyPanel.Visible = (reports.Count = 0)
        End Sub

        Protected Sub rptReports_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
            If e.CommandName = "Reviewed" Then
                Dim reportId = Convert.ToInt32(e.CommandArgument)
                If ServiceReport.MarkReviewed(reportId, CurrentAdmin.UserId) Then
                    AdminActionLog.Log(CurrentAdmin.UserId, "Переглянув скаргу", "Скарга #" & reportId)
                    ShowInfo("Скаргу позначено переглянутою.")
                End If
            End If

            BindReports()
        End Sub

        Private Sub ShowInfo(message As String)
            infoLabel.Text = message
            infoLabel.Visible = True
        End Sub

    End Class

End Namespace
