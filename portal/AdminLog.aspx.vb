Imports System

Namespace SumyPortal

    ''' <summary>
    ''' Перегляд журналу дій адміна (п.10 уточненої постановки, 2026-09-09) —
    ''' лише читання, об'єднує AdminActionLog і ModerationLog (App_Code/AdminLog.vb).
    ''' </summary>
    Public Class AdminLog
        Inherits AdminPageBase

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not IsPostBack Then
                Dim entries = AdminActionLog.GetRecent()
                rptLog.DataSource = entries
                rptLog.DataBind()
                emptyPanel.Visible = (entries.Count = 0)
            End If
        End Sub

    End Class

End Namespace
