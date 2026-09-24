Imports System
Imports System.Collections.Generic
Imports System.IO

Namespace SumyPortal

    Public Class MyServices
        Inherits ProviderPageBase

        Protected Overrides Sub InitializeCulture()
            LocalizationHelper.ApplyCulture(Me)
            MyBase.InitializeCulture()
        End Sub

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not IsPostBack Then
                BindServices()
            End If
        End Sub

        Private Sub BindServices()
            Dim services = Service.GetByProvider(CurrentProvider.UserId)

            ' Статистика для постачальника (п.17, наступна фіча понад MVP, 2026-09-12) —
            ' розмови/вподобання/відгуки рахуються "на льоту" з наявних таблиць, той самий
            ' прийом, що ThumbnailUrl у Catalog.aspx.vb (властивості лише для цієї сторінки,
            ' Service.Map(reader) їх не заповнює). ViewCount уже прийшов зі свого стовпця.
            For Each svc In services
                svc.ConversationCount = Service.GetConversationCount(svc.ServiceId)
                svc.FavoriteCount = Favorite.GetCount(svc.ServiceId)
                Dim average As Decimal? = Nothing
                Dim count As Integer
                Review.GetSummary(svc.ServiceId, average, count)
                svc.ReviewAverage = average
                svc.ReviewCount = count
            Next

            Dim expiryDates = Service.GetExpiryDates(CurrentProvider.UserId, Service.StaleDays)
            For Each svc In services
                Dim expiresAt As DateTime
                If expiryDates.TryGetValue(svc.ServiceId, expiresAt) Then svc.ExpiresAt = expiresAt
            Next

            rptServices.DataSource = services
            rptServices.DataBind()
            emptyPanel.Visible = (services.Count = 0)
        End Sub

        ''' <summary>Формує рядок статистики під карткою — окремий Literal у шаблоні, бо
        ''' форматування (плюралізація/decimal) зручніше зібрати тут, ніж кількома
        ''' окремими `&lt;%#: %&gt;`-виразами в розмітці.</summary>
        Protected Sub rptServices_ItemDataBound(sender As Object, e As RepeaterItemEventArgs)
            If e.Item.ItemType <> ListItemType.Item AndAlso e.Item.ItemType <> ListItemType.AlternatingItem Then Return

            Dim svc = CType(e.Item.DataItem, Service)
            BindExpiry(e.Item, svc)

            Dim statsLiteral = CType(e.Item.FindControl("statsLiteral"), Literal)
            If statsLiteral Is Nothing Then Return

            Dim parts As New List(Of String) From {
                String.Format(Resources.SiteText.MyServices_Stats_Views, svc.ViewCount),
                String.Format(Resources.SiteText.MyServices_Stats_Conversations, svc.ConversationCount),
                String.Format(Resources.SiteText.MyServices_Stats_Favorites, svc.FavoriteCount),
                If(svc.ReviewAverage.HasValue,
                    String.Format(Resources.SiteText.MyServices_Stats_ReviewsRated, svc.ReviewAverage.Value, svc.ReviewCount),
                    Resources.SiteText.MyServices_Stats_ReviewsNone)
            }
            statsLiteral.Text = String.Join(" · ", parts)
        End Sub

        ''' <summary>«Активне до …» для опублікованих; за StaleWarningDays до автозняття —
        ''' попереджувальний стиль і кнопка «Продовжити» (раніше не показуємо, щоб список не
        ''' перетворювався на щоденне «клікни продовжити»).</summary>
        Private Sub BindExpiry(item As RepeaterItem, svc As Service)
            If Not svc.ExpiresAt.HasValue Then Return

            Dim expiryLabel = CType(item.FindControl("expiryLabel"), Label)
            Dim renewButton = CType(item.FindControl("renewButton"), LinkButton)
            Dim expiringSoon = svc.ExpiresAt.Value <= DateTime.UtcNow.AddDays(Service.StaleWarningDays)

            expiryLabel.Visible = True
            expiryLabel.CssClass = If(expiringSoon, "form-error", "service-category")
            expiryLabel.Text = String.Format(
                If(expiringSoon, Resources.SiteText.MyServices_ExpiresSoon, Resources.SiteText.MyServices_ActiveUntil),
                svc.ExpiresAt.Value)

            renewButton.Visible = expiringSoon
            renewButton.Text = String.Format(Resources.SiteText.MyServices_Renew, Service.StaleDays)
        End Sub

        Protected Sub rptServices_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
            Dim serviceId = Convert.ToInt32(e.CommandArgument)
            Dim ok As Boolean

            Select Case e.CommandName
                Case "Submit"
                    ok = Service.SubmitForModeration(serviceId, CurrentProvider.UserId)
                    If ok Then
                        ShowInfo(Resources.SiteText.MyServices_Msg_Submitted)
                    End If
                Case "Unpublish"
                    ok = Service.Unpublish(serviceId, CurrentProvider.UserId)
                    If ok Then
                        ShowInfo(Resources.SiteText.MyServices_Msg_Unpublished)
                    End If
                Case "Renew"
                    ok = Service.Renew(serviceId, CurrentProvider.UserId)
                    If ok Then
                        ShowInfo(String.Format(Resources.SiteText.MyServices_Msg_Renewed, Service.StaleDays))
                    End If
                Case "Delete"
                    ok = Service.Delete(serviceId, CurrentProvider.UserId)
                    If ok Then
                        DeleteServiceDirectory(serviceId)
                        ShowInfo(Resources.SiteText.MyServices_Msg_Deleted)
                    End If
            End Select

            BindServices()
        End Sub

        ''' <summary>Фото й специфікація лежать в одній теці оголошення — прибираємо її цілком
        ''' (той самий підхід, що AdminUsers.aspx.vb). Рядки в БД уже видалено, тому помилка
        ''' файлової системи не критична.</summary>
        Private Sub DeleteServiceDirectory(serviceId As Integer)
            Try
                Dim dir = Server.MapPath("~/Uploads/Services/" & serviceId & "/")
                If Directory.Exists(dir) Then Directory.Delete(dir, True)
            Catch
            End Try
        End Sub

        Private Sub ShowInfo(message As String)
            infoLabel.Text = message
            infoLabel.Visible = True
        End Sub

    End Class

End Namespace
