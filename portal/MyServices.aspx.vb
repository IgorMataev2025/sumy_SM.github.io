Imports System
Imports System.Collections.Generic

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
            End Select

            BindServices()
        End Sub

        Private Sub ShowInfo(message As String)
            infoLabel.Text = message
            infoLabel.Visible = True
        End Sub

    End Class

End Namespace
