Imports System
Imports System.Collections.Generic

Namespace SumyPortal

    ''' <summary>Донати LiqPay для адміна (2026-09-25, аудит Адміна, п.6) — таблиця Donations
    ''' (Donate.aspx/DonationCallback.aspx) досі не мала жодного перегляду в адмінці.</summary>
    Public Class AdminDonations
        Inherits AdminPageBase

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not IsPostBack Then
                If ddlStatus.Items.FindByValue(QueryParam("status")) IsNot Nothing Then ddlStatus.SelectedValue = QueryParam("status")
                sum30Literal.Text = AdminData.DonationsSum(30).ToString("0.##")
                sumAllLiteral.Text = AdminData.DonationsSum(Nothing).ToString("0.##")

                Dim total As Integer
                Dim items = AdminData.GetDonations(QueryParam("status"), CurrentPageNumber, AdminPageSize, total)
                rptDonations.DataSource = items
                rptDonations.DataBind()
                emptyPanel.Visible = (items.Count = 0)
                BindPager(lnkPrev, lnkNext, pageInfoLabel, total, New Dictionary(Of String, String) From {{"status", QueryParam("status")}})
            End If
        End Sub

        Protected Sub btnSearch_Click(sender As Object, e As EventArgs)
            RedirectWithParams(New Dictionary(Of String, String) From {{"status", ddlStatus.SelectedValue}})
        End Sub

    End Class

End Namespace
