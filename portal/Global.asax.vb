Imports System
Imports System.Web

Namespace SumyPortal

    Public Class [Global]
        Inherits HttpApplication

        Sub Application_Start(sender As Object, e As EventArgs)
            ' Ініціалізація на старті застосунку (кеші довідників тощо — пізніше).
        End Sub

        ''' <summary>
        ''' Голий корінь застосунку ("/") IIS резолвить у Default.aspx як default
        ''' document лише на стадії ResolveRequestCache — вона виконується ПІСЛЯ
        ''' AuthenticateRequest/AuthorizeRequest, де вже спрацьовує ASP.NET-
        ''' авторизація з Web.config. Тобто &lt;location path="Default.aspx"&gt;
        ''' (allow users="*", відкритий перегляд для USER, п.12 уточненої
        ''' постановки) до цього моменту ще не бачить, що запит насправді на
        ''' Default.aspx — Request.Path усе ще "/" — і анонімного відвідувача
        ''' редіректить на Login.aspx, хоча "/Default.aspx" напряму відкривався
        ''' нормально (знайдено живим тестом 2026-09-11). Application_BeginRequest
        ''' виконується найпершим у конвеєрі — переписуємо шлях тут, до того як
        ''' дійде до авторизації.
        ''' </summary>
        Sub Application_BeginRequest(sender As Object, e As EventArgs)
            Dim appPath = Request.ApplicationPath
            If String.IsNullOrEmpty(appPath) Then appPath = "/"
            If Not appPath.EndsWith("/") Then appPath &= "/"

            If String.Equals(Request.Path, appPath, StringComparison.OrdinalIgnoreCase) Then
                ' 3-аргументний overload (а не однорядковий RewritePath("~/Default.aspx"))
                ' — однорядковий мовчки ОЧИЩУЄ рядок запиту, коли в переданому шляху
                ' немає "?" (напр. https://site/?utm_source=... втратив би параметри).
                HttpContext.Current.RewritePath("~/Default.aspx", String.Empty, Request.QueryString.ToString())
            End If
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
