Imports System
Imports MySql.Data.MySqlClient

Namespace SumyPortal

    Public Class [Default]
        Inherits System.Web.UI.Page

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
