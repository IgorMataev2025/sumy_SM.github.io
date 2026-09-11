Imports System

Namespace SumyPortal

    ''' <summary>
    ''' Єдина точка входу в адмінку (концептуальне рішення 2026-09-09): сторінки
    ''' Moderation/Categories/Users більше не мають посилань у Site.master
    ''' (щоб адмінська частина не змішувалась зі звичайним меню сайту), адмін
    ''' заходить сюди напряму за URL і звідси переходить у потрібний розділ.
    ''' </summary>
    Public Class AdminDashboard
        Inherits AdminPageBase

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not IsPostBack Then
                Dim pendingCount = Service.GetPendingForModeration().Count
                pendingCountLiteral.Text = If(pendingCount = 0,
                    "Черга порожня.",
                    pendingCount.ToString() & " оголошення(нь) очікує(ють) рішення.")

                totalServicesCountLiteral.Text = Service.GetAllForAdmin().Count.ToString() & " оголошень усіх статусів — прямий CRUD."

                categoryCountLiteral.Text = ServiceCategory.GetAllCategories().Count.ToString() & " категорій (активних і деактивованих)."

                userCountLiteral.Text = UserAccount.GetAll().Count.ToString() & " зареєстрованих користувачів."

                Dim logCount = AdminActionLog.GetRecent().Count
                logCountLiteral.Text = If(logCount = 0, "Журнал порожній.", logCount.ToString() & " записів (останні дії адміністраторів).")
            End If
        End Sub

    End Class

End Namespace
