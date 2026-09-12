Imports System

Namespace SumyPortal

    ''' <summary>
    ''' result_url для LiqPay (п.1 уточненої постановки) — сюди браузер
    ''' користувача повертається після оплати. Це ЛИШЕ інформаційне
    ''' повідомлення: остаточний статус донату вирішує DonationCallback.aspx
    ''' (server_url), бо тут довіри вхідним даним немає — сторінку можна
    ''' відкрити з довільним order_id в URL. Публічна (Web.config, location).
    ''' </summary>
    Public Class DonationResult
        Inherits System.Web.UI.Page

        Protected Overrides Sub InitializeCulture()
            LocalizationHelper.ApplyCulture(Me)
            MyBase.InitializeCulture()
        End Sub

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If IsPostBack Then Return

            Dim orderId = Request.QueryString("order_id")
            If String.IsNullOrEmpty(orderId) Then
                notFoundPanel.Visible = True
                Return
            End If

            Dim record = Donation.GetByOrderId(orderId)
            If record Is Nothing Then
                notFoundPanel.Visible = True
                Return
            End If

            Select Case record.Status
                Case "Success"
                    successTextLiteral.Text = String.Format(Resources.SiteText.DonationResult_Success,
                        Server.HtmlEncode(record.Amount.ToString("0.## грн")))
                    successPanel.Visible = True
                Case "Failure"
                    failurePanel.Visible = True
                Case Else
                    pendingPanel.Visible = True
            End Select
        End Sub

    End Class

End Namespace
