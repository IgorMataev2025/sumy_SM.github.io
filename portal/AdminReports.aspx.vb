Imports System
Imports System.Linq
Imports System.Web.UI.WebControls

Namespace SumyPortal

    ''' <summary>
    ''' Черга відкритих скарг на оголошення (наступна фіча понад MVP, обрано
    ''' автономно циклом /loop, 2026-09-13) — сигнал адміну від будь-кого
    ''' (включно з анонімом), не автоматична модерація.
    ''' 2026-09-25 (аудит Адміна, п.3): скарги згруповані за оголошенням, і рішення
    ''' приймається прямо тут — «Зняти оголошення» (Approved → Rejected з причиною,
    ''' лист постачальнику) або «Скарги безпідставні»; обидві дії закривають усі
    ''' відкриті скарги групи й пишуться в журнал.
    ''' </summary>
    Public Class AdminReports
        Inherits AdminPageBase

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not IsPostBack Then
                BindReports()
            End If
        End Sub

        Private Sub BindReports()
            Dim groups = ServiceReport.GetOpenGrouped()
            rptGroups.DataSource = groups
            rptGroups.DataBind()
            emptyPanel.Visible = (groups.Count = 0)
        End Sub

        Protected Sub rptGroups_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
            Dim serviceId = Convert.ToInt32(e.CommandArgument)
            Dim group = ServiceReport.GetOpenGrouped().FirstOrDefault(Function(g) g.ServiceId = serviceId)
            If group Is Nothing Then
                BindReports()
                Return
            End If

            Select Case e.CommandName
                Case "TakeDown"
                    Dim txt = TryCast(e.Item.FindControl("txtTakeDownReason"), TextBox)
                    Dim reason = If(txt IsNot Nothing, txt.Text.Trim(), String.Empty)
                    If String.IsNullOrEmpty(reason) Then
                        reason = "Знято за скаргами користувачів: " &
                                 String.Join(", ", group.Reports.Select(Function(r) r.ReasonLabel).Distinct())
                    End If

                    Dim svc = Service.AdminTakeDown(serviceId, CurrentAdmin.UserId, reason)
                    If svc Is Nothing Then
                        ShowInfo("Оголошення вже не опубліковане — скарги лишились відкритими, оновіть сторінку.")
                    Else
                        ServiceReport.MarkReviewedForService(serviceId, CurrentAdmin.UserId)
                        AdminActionLog.Log(CurrentAdmin.UserId, "Зняв оголошення за скаргами", svc.Title & " — " & reason)
                        Try
                            EmailSender.Send(svc.ProviderEmail, "Ваше оголошення знято з публікації — Safina",
                                String.Format("Ваше оголошення «{0}» знято з публікації адміністратором за скаргами користувачів.{1}Причина: {2}{1}{1}Ви можете виправити оголошення в «Мої оголошення» і подати його на модерацію знову.",
                                              svc.Title, Environment.NewLine, reason))
                        Catch
                        End Try
                        ShowInfo("Оголошення знято, постачальника повідомлено, скарги закрито.")
                    End If

                Case "Dismiss"
                    Dim closed = ServiceReport.MarkReviewedForService(serviceId, CurrentAdmin.UserId)
                    AdminActionLog.Log(CurrentAdmin.UserId, "Відхилив скарги як безпідставні", group.ServiceTitle & " (скарг: " & closed & ")")
                    ShowInfo("Скарги закрито, оголошення без змін.")
            End Select

            BindReports()
        End Sub

        Private Sub ShowInfo(message As String)
            infoLabel.Text = message
            infoLabel.Visible = True
        End Sub

    End Class

End Namespace
