Imports System
Imports System.Web.Security

Namespace SumyPortal

    Public Class Logout
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            FormsAuthentication.SignOut()
            Response.Redirect("~/Login.aspx", False)
        End Sub

    End Class

End Namespace
