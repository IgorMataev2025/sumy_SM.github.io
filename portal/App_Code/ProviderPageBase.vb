Imports System

Namespace SumyPortal

    ''' <summary>
    ''' Базовий клас для сторінок особистого кабінету постачальника (ТЗ, розділ 4.2).
    ''' Сторінка вже захищена Forms-автентифікацією (Web.config, deny users="?"),
    ''' тут додатково перевіряється роль — Consumer на ці сторінки не пускаємо.
    ''' </summary>
    Public MustInherit Class ProviderPageBase
        Inherits System.Web.UI.Page

        Protected CurrentProvider As UserAccount

        Protected Overrides Sub OnInit(e As EventArgs)
            MyBase.OnInit(e)

            CurrentProvider = UserAccount.FindByEmail(Page.User.Identity.Name)

            If CurrentProvider Is Nothing OrElse CurrentProvider.UserType <> "Provider" Then
                Response.Redirect("~/Default.aspx", True)
            End If
        End Sub

    End Class

End Namespace
