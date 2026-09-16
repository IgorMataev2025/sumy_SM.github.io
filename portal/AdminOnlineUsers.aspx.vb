Imports System
Imports System.Configuration

Namespace SumyPortal

    ''' <summary>Моніторинг залогінених користувачів у реальному часі (2026-09-16, прямий
    ''' запит користувача). "Онлайн" — LastActivityAt (Global.asax: TouchLastActivity,
    ''' throttled) не старіший за OnlineThresholdMinutes (Web.config). Сторінка сама
    ''' оновлюється через &lt;meta http-equiv="refresh"&gt; — простіше за AJAX-опитування
    ''' (MessagesPoll.ashx/CallSignal.ashx), і достатньо для внутрішнього адмін-інструмента,
    ''' де немає потреби уникати перезавантаження сторінки.</summary>
    Public Class AdminOnlineUsers
        Inherits AdminPageBase

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            Dim minutes As Integer
            If Not Integer.TryParse(ConfigurationManager.AppSettings("OnlineThresholdMinutes"), minutes) Then minutes = 5

            Dim onlineUsers = UserAccount.GetOnlineUsers(minutes)
            rptOnlineUsers.DataSource = onlineUsers
            rptOnlineUsers.DataBind()

            emptyPanel.Visible = (onlineUsers.Count = 0)
            thresholdLiteral.Text = "Онлайн — хто заходив на сайт за останні " & minutes & " хв."
            refreshedAtLiteral.Text = DateTime.UtcNow.ToString("HH:mm:ss")
        End Sub

        ''' <summary>Той самий принцип відображення імені, що Profile.aspx: для юросіб
        ''' пріоритет CompanyName, інакше FullName.</summary>
        Public Shared Function DisplayName(account As UserAccount) As String
            If account.IsLegalEntity AndAlso Not String.IsNullOrWhiteSpace(account.CompanyName) Then
                Return account.CompanyName
            End If
            Return account.FullName
        End Function

        Public Shared Function FormatMinutesAgo(lastActivityAt As DateTime?) As String
            If Not lastActivityAt.HasValue Then Return "—"

            Dim minutesAgo = CInt(Math.Floor(DateTime.UtcNow.Subtract(lastActivityAt.Value).TotalMinutes))
            If minutesAgo <= 0 Then Return "щойно"
            Return minutesAgo & " хв. тому"
        End Function

    End Class

End Namespace
