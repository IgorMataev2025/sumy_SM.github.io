Imports System
Imports MySql.Data.MySqlClient

Namespace SumyPortal

    Public Class [Default]
        Inherits System.Web.UI.Page

        ''' <summary>Багатомовність (постановка робочої тестової версії, 2026-09-12) —
        ''' офіційна точка ASP.NET Web Forms для програмної культури, до того як
        ''' вона "застигне" на задекларованому в Web.config значенні (uk-UA).</summary>
        Protected Overrides Sub InitializeCulture()
            LocalizationHelper.ApplyCulture(Me)
            MyBase.InitializeCulture()
        End Sub

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not IsPostBack Then
                Try
                    rptCategories.DataSource = ServiceCategory.GetActiveCategories()
                    rptCategories.DataBind()
                Catch ex As MySqlException
                    ' БД тимчасово недоступна — не валимо сторінку 500-ю, показуємо повідомлення.
                    dbErrorPanel.Visible = True
                    rptCategories.Visible = False
                End Try
            End If
        End Sub

    End Class

End Namespace
