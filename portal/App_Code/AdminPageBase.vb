Imports System

Namespace SumyPortal

    ''' <summary>
    ''' Базовий клас для сторінок адмін-панелі (ТЗ, розділ 4.3). Сторінка вже
    ''' захищена Forms-автентифікацією (Web.config, deny users="?"), тут
    ''' додатково перевіряється прапорець IsAdmin — інших не пускаємо.
    ''' </summary>
    Public MustInherit Class AdminPageBase
        Inherits System.Web.UI.Page

        Protected CurrentAdmin As UserAccount

        Protected Overrides Sub OnInit(e As EventArgs)
            MyBase.OnInit(e)

            CurrentAdmin = UserAccount.FindByEmail(Page.User.Identity.Name)

            If CurrentAdmin Is Nothing OrElse Not CurrentAdmin.IsAdmin Then
                Response.Redirect("~/Default.aspx", True)
            End If
        End Sub

    End Class

End Namespace
