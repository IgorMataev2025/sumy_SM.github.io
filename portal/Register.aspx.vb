Imports System

Namespace SumyPortal

    ''' <summary>
    ''' Сторінка-розвилка: реєстрація сама по собі відбувається на
    ''' RegisterConsumer.aspx / RegisterProvider.aspx (розділено за вимогою
    ''' постановки — Постачальник і Споживач мають окремі форми).
    ''' </summary>
    Public Class Register
        Inherits System.Web.UI.Page

        Protected Overrides Sub InitializeCulture()
            LocalizationHelper.ApplyCulture(Me)
            MyBase.InitializeCulture()
        End Sub
    End Class

End Namespace
