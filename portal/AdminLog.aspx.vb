Imports System
Imports System.Collections.Generic
Imports System.Globalization

Namespace SumyPortal

    ''' <summary>
    ''' Перегляд журналу дій адміна (п.10 уточненої постановки, 2026-09-09) —
    ''' лише читання, об'єднує AdminActionLog і ModerationLog.
    ''' 2026-09-25 (аудит Адміна, п.9): пошук за текстом і датами + сторінки
    ''' (AdminActionLog.Search) замість лише останніх 200 записів.
    ''' </summary>
    Public Class AdminLog
        Inherits AdminPageBase

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not IsPostBack Then
                txtSearch.Text = QueryParam("q")
                txtFrom.Text = QueryParam("from")
                txtTo.Text = QueryParam("to")

                Dim total As Integer
                Dim entries = AdminActionLog.Search(QueryParam("q"), ParseDate(QueryParam("from")), ParseDate(QueryParam("to")),
                                                    CurrentPageNumber, AdminPageSize, total)
                rptLog.DataSource = entries
                rptLog.DataBind()
                emptyPanel.Visible = (entries.Count = 0)
                BindPager(lnkPrev, lnkNext, pageInfoLabel, total, New Dictionary(Of String, String) From {
                    {"q", QueryParam("q")}, {"from", QueryParam("from")}, {"to", QueryParam("to")}})
            End If
        End Sub

        Private Shared Function ParseDate(value As String) As DateTime?
            Dim d As DateTime
            If DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, d) Then Return d
            Return Nothing
        End Function

        Protected Sub btnSearch_Click(sender As Object, e As EventArgs)
            RedirectWithParams(New Dictionary(Of String, String) From {
                {"q", txtSearch.Text}, {"from", txtFrom.Text}, {"to", txtTo.Text}})
        End Sub

    End Class

End Namespace
