Imports System

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
            rptServices.DataSource = services
            rptServices.DataBind()
            emptyPanel.Visible = (services.Count = 0)
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
