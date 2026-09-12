Imports System

Namespace SumyPortal

    ''' <summary>
    ''' Публічна статична сторінка юридичної довідки (постановка робочої тестової
    ''' версії, 2026-09-12) — права Споживача, вимоги до Постачальника (ФОП/юрособа),
    ''' порядок вирішення спорів. Доступна без входу, окремий пункт меню (Site.master).
    ''' </summary>
    Public Class LegalGuide
        Inherits System.Web.UI.Page

        ''' <summary>Базове SEO (п.21, наступна фіча понад MVP, 2026-09-12).</summary>
        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            Master.MetaDescription = "Юридична довідка порталу Safina — права споживача, " &
                "вимоги до постачальника (ФОП/юрособа), порядок вирішення спорів."
        End Sub

    End Class

End Namespace
