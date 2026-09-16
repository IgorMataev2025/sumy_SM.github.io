Imports System
Imports System.Configuration

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

                Dim openReportsCount = ServiceReport.GetOpen().Count
                reportsCountLiteral.Text = If(openReportsCount = 0,
                    "Відкритих скарг немає.",
                    openReportsCount.ToString() & " відкрита(их) скарга(и) від відвідувачів.")

                categoryCountLiteral.Text = ServiceCategory.GetAllCategories().Count.ToString() & " категорій (активних і деактивованих)."

                userCountLiteral.Text = UserAccount.GetAll().Count.ToString() & " зареєстрованих користувачів."

                Dim logCount = AdminActionLog.GetRecent().Count
                logCountLiteral.Text = If(logCount = 0, "Журнал порожній.", logCount.ToString() & " записів (останні дії адміністраторів).")

                Dim onlineMinutes As Integer
                If Not Integer.TryParse(ConfigurationManager.AppSettings("OnlineThresholdMinutes"), onlineMinutes) Then onlineMinutes = 5
                Dim onlineCount = UserAccount.GetOnlineUsers(onlineMinutes).Count
                onlineCountLiteral.Text = If(onlineCount = 0,
                    "Онлайн зараз немає нікого.",
                    onlineCount.ToString() & " користувач(ів) онлайн зараз.")
            End If
        End Sub

    End Class

End Namespace
