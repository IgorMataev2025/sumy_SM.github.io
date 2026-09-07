Imports System

Namespace SumyPortal

    ''' <summary>
    ''' server_url для LiqPay (п.1 уточненої постановки) — сюди LiqPay
    ''' надсилає server-to-server POST одразу після обробки платежу.
    ''' Підпис ОБОВ'ЯЗКОВО перевіряється тут (result_url, куди повертається
    ''' браузер користувача, довіри не заслуговує — його може відкрити хто
    ''' завгодно з довільним order_id). Саме цей файл робить статус донату
    ''' остаточним у БД. Публічний (Web.config, location) — LiqPay не має
    ''' облікового запису на порталі.
    ''' </summary>
    Public Class DonationCallback
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            Dim data = Request.Form("data")
            Dim signature = Request.Form("signature")

            If String.IsNullOrEmpty(data) OrElse String.IsNullOrEmpty(signature) Then
                Response.StatusCode = 400
                Response.End()
                Return
            End If

            If Not LiqPayHelper.VerifySignature(data, signature) Then
                Response.StatusCode = 400
                Response.End()
                Return
            End If

            Dim json = LiqPayHelper.DecodeData(data)
            Dim orderId = LiqPayHelper.ExtractField(json, "order_id")
            Dim liqPayStatus = LiqPayHelper.ExtractField(json, "status")
            Dim paymentId = LiqPayHelper.ExtractField(json, "payment_id")

            If Not String.IsNullOrEmpty(orderId) Then
                Donation.UpdateStatus(orderId, MapStatus(liqPayStatus), liqPayStatus, paymentId)
            End If

            Response.StatusCode = 200
            Response.End()
        End Sub

        ''' <summary>Статуси LiqPay (https://www.liqpay.ua/documentation/) звужуємо до трьох власних.
        ''' Проміжні (wait_*, processing тощо) лишаємо Pending — наступний callback уточнить.</summary>
        Private Function MapStatus(liqPayStatus As String) As String
            Select Case liqPayStatus
                Case "success", "sandbox"
                    Return "Success"
                Case "failure", "error"
                    Return "Failure"
                Case Else
                    Return "Pending"
            End Select
        End Function

    End Class

End Namespace
