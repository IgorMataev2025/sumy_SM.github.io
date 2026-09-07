Imports System
Imports System.Web

Namespace SumyPortal

    Public Class [Global]
        Inherits HttpApplication

        Sub Application_Start(sender As Object, e As EventArgs)
            ' Ініціалізація на старті застосунку (кеші довідників тощо — пізніше).
        End Sub

        Sub Session_Start(sender As Object, e As EventArgs)
        End Sub

        Sub Application_Error(sender As Object, e As EventArgs)
            ' TODO: логування помилок (файл/БД) перед виходом на бойовий хостинг.
        End Sub

        Sub Session_End(sender As Object, e As EventArgs)
        End Sub

        Sub Application_End(sender As Object, e As EventArgs)
        End Sub

    End Class

End Namespace
