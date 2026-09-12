Imports System
Imports System.Web

Namespace SumyPortal

    ''' <summary>
    ''' Багатомовність (постановка робочої тестової версії, 2026-09-12) —
    ''' лише публічні сторінки (Default/Catalog/ServiceDetails), лише статичний
    ''' інтерфейс (не контент від користувачів — назви/описи оголошень і
    ''' категорій лишаються українською незалежно від мови). Українська —
    ''' за замовчуванням.
    '''
    ''' Кожна сторінка в межах цієї фічі має перевизначити Page.InitializeCulture()
    ''' і викликати ApplyCulture(Me) — це єдина офіційна точка ASP.NET Web Forms
    ''' для програмного встановлення культури до того, як вона "застигне" на
    ''' задекларованому в Web.config значенні (uk-UA). Сторінки поза цією фічею
    ''' перевизначення не мають — вони й далі завжди українською (свідоме
    ''' обмеження обсягу, а не недогляд).
    ''' </summary>
    Public NotInheritable Class LocalizationHelper

        Public Const CookieName As String = "PreferredCulture"
        Public Const EnglishCulture As String = "en"
        Public Const DefaultCulture As String = "uk-UA"

        ''' <summary>Викликати з перевизначеного Page.InitializeCulture() кожної сторінки в обсязі фічі.</summary>
        Public Shared Sub ApplyCulture(page As System.Web.UI.Page)
            Dim culture = ResolveCulture(page.Request)
            page.UICulture = culture
            page.Culture = culture
        End Sub

        Private Shared Function ResolveCulture(request As HttpRequest) As String
            Dim cookie = request.Cookies(CookieName)
            If cookie IsNot Nothing AndAlso cookie.Value = EnglishCulture Then
                Return EnglishCulture
            End If
            Return DefaultCulture
        End Function

    End Class

End Namespace
