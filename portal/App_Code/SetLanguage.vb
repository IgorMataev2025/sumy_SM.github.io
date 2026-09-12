Imports System
Imports System.Web

Namespace SumyPortal

    ''' <summary>
    ''' Обробник перемикання мови (постановка робочої тестової версії, 2026-09-12) —
    ''' лише зберігає вибір у cookie й повертає відвідувача туди, звідки прийшов.
    ''' Клас лежить в App_Code (не окремий CodeBehind) — стандартний прийом для
    ''' .ashx у файловій моделі Website Project (той самий підхід, що вже для
    ''' .aspx.vb з CodeFile=, а не CodeBehind=, — грабля з README.md).
    ''' </summary>
    Public Class SetLanguage
        Implements IHttpHandler

        Public ReadOnly Property IsReusable As Boolean Implements IHttpHandler.IsReusable
            Get
                Return True
            End Get
        End Property

        Public Sub ProcessRequest(context As HttpContext) Implements IHttpHandler.ProcessRequest
            Dim lang = context.Request.QueryString("lang")
            Dim cookie As New HttpCookie(LocalizationHelper.CookieName)
            cookie.Value = If(lang = LocalizationHelper.EnglishCulture, LocalizationHelper.EnglishCulture, "uk")
            cookie.Expires = DateTime.UtcNow.AddYears(1)
            cookie.Path = "/"
            context.Response.Cookies.Add(cookie)

            ' Referer — та сама сторінка, звідки клікнули UA/EN; якщо його нема
            ' (напр. прямий заход на обробник), повертаємо на головну.
            Dim returnUrl = context.Request.UrlReferrer
            If returnUrl IsNot Nothing Then
                context.Response.Redirect(returnUrl.PathAndQuery, True)
            Else
                context.Response.Redirect("~/Default.aspx", True)
            End If
        End Sub

    End Class

End Namespace
