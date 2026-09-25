Imports System
Imports System.Collections.Generic
Imports System.Web.UI.WebControls

Namespace SumyPortal

    ''' <summary>Загальний список відгуків з видаленням (2026-09-25, аудит Адміна, п.7).</summary>
    Public Class AdminReviews
        Inherits AdminPageBase

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not IsPostBack Then
                If ddlRating.Items.FindByValue(QueryParam("maxRating")) IsNot Nothing Then ddlRating.SelectedValue = QueryParam("maxRating")
                BindReviews()
            End If
        End Sub

        Private Sub BindReviews()
            Dim maxRating As Integer
            Dim filter As Integer? = If(Integer.TryParse(QueryParam("maxRating"), maxRating), maxRating, CType(Nothing, Integer?))
            Dim total As Integer
            Dim items = AdminData.GetRecentReviews(filter, CurrentPageNumber, AdminPageSize, total)
            rptReviews.DataSource = items
            rptReviews.DataBind()
            emptyPanel.Visible = (items.Count = 0)
            BindPager(lnkPrev, lnkNext, pageInfoLabel, total, New Dictionary(Of String, String) From {{"maxRating", QueryParam("maxRating")}})
        End Sub

        Protected Sub btnSearch_Click(sender As Object, e As EventArgs)
            RedirectWithParams(New Dictionary(Of String, String) From {{"maxRating", ddlRating.SelectedValue}})
        End Sub

        Protected Sub rptReviews_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
            If e.CommandName = "Delete" Then
                Dim reviewId = Convert.ToInt32(e.CommandArgument)
                If Review.AdminDelete(reviewId) Then
                    AdminActionLog.Log(CurrentAdmin.UserId, "Видалив відгук", "Відгук #" & reviewId)
                    infoLabel.Text = "Відгук видалено."
                    infoLabel.Visible = True
                End If
            End If
            BindReviews()
        End Sub

    End Class

End Namespace
