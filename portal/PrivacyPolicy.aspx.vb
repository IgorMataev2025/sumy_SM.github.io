Imports System

Namespace SumyPortal

    ''' <summary>
    ''' Публічна статична сторінка політики конфіденційності (п.4 уточненої
    ''' постановки — розділ 10 ТЗ, "Політика обробки персональних даних").
    ''' Доступна без входу — на неї посилаються форми реєстрації й футер.
    ''' </summary>
    Public Class PrivacyPolicy
        Inherits System.Web.UI.Page

        ''' <summary>Розширене SEO (2026-09-22), той самий принцип, що LegalGuide.aspx.vb (п.21).</summary>
        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            Master.MetaDescription = "Політика конфіденційності порталу Safina — які дані " &
                "збираються, з якою метою і як захищаються персональні дані користувачів."
            Master.OgTitle = "Політика конфіденційності — Портал послуг Safina"
        End Sub

    End Class

End Namespace
