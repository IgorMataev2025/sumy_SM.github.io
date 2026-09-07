Imports System
Imports System.Globalization

Namespace SumyPortal

    ''' <summary>
    ''' Публічна сторінка донату (п.1 уточненої постановки — "переказ коштів
    ''' на підтримку проєкту"). Доступна БЕЗ входу (Web.config, location) —
    ''' підтримати проєкт може будь-хто, не лише зареєстровані користувачі.
    ''' </summary>
    Public Class Donate
        Inherits System.Web.UI.Page

        Protected Sub btnDonate_Click(sender As Object, e As EventArgs)
            If Not Page.IsValid Then Return

            Dim amount As Decimal
            If rblAmount.SelectedValue = "custom" Then
                If Not Decimal.TryParse(txtCustomAmount.Text, NumberStyles.Number, CultureInfo.InvariantCulture, amount) Then
                    ShowError("Вкажіть коректну суму.")
                    Return
                End If
            Else
                amount = Decimal.Parse(rblAmount.SelectedValue, CultureInfo.InvariantCulture)
            End If

            If amount < 1 OrElse amount > 100000 Then
                ShowError("Сума має бути від 1 до 100 000 грн.")
                Return
            End If

            ' Унікальний ID замовлення для LiqPay і власного трекінгу статусу.
            Dim orderId = "DON" & DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture)
            Donation.Create(orderId, amount, txtDonorName.Text.Trim(), txtDonorEmail.Text.Trim())

            Dim baseUrl = Request.Url.GetLeftPart(UriPartial.Authority)
            Dim resultUrl = baseUrl & ResolveUrl("~/DonationResult.aspx") & "?order_id=" & Server.UrlEncode(orderId)
            Dim serverUrl = baseUrl & ResolveUrl("~/DonationCallback.aspx")

            Dim data As String = Nothing
            Dim signature As String = Nothing
            LiqPayHelper.BuildCheckout(orderId, amount, "Підтримка порталу Safina", resultUrl, serverUrl, data, signature)

            RenderCheckoutRedirect(data, signature)
        End Sub

        ''' <summary>
        ''' Пише власну мінімальну HTML-сторінку з формою, що POST-ить прямо на
        ''' LiqPay (https://www.liqpay.ua/api/3/checkout) — в обхід серверної
        ''' <form runat="server"> зі Site.master (HTML не дозволяє вкладені
        ''' форми, а ця має вести на зовнішній домен, а не постбек на себе).
        ''' </summary>
        Private Sub RenderCheckoutRedirect(data As String, signature As String)
            Response.Clear()
            Response.ContentType = "text/html; charset=utf-8"
            Response.Write("<!DOCTYPE html><html lang=""uk""><head><meta charset=""utf-8""><title>Перенаправлення на LiqPay…</title></head><body>")
            Response.Write("<p>Перенаправлення на сторінку оплати LiqPay…</p>")
            Response.Write("<form id=""liqpayForm"" method=""POST"" action=""https://www.liqpay.ua/api/3/checkout"" accept-charset=""utf-8"">")
            Response.Write("<input type=""hidden"" name=""data"" value=""" & Server.HtmlEncode(data) & """ />")
            Response.Write("<input type=""hidden"" name=""signature"" value=""" & Server.HtmlEncode(signature) & """ />")
            Response.Write("<noscript><button type=""submit"">Перейти до оплати</button></noscript>")
            Response.Write("</form>")
            Response.Write("<script>document.getElementById('liqpayForm').submit();</script>")
            Response.Write("</body></html>")
            Response.End()
        End Sub

        Private Sub ShowError(message As String)
            serverErrorLabel.Text = message
            serverErrorLabel.Visible = True
        End Sub

    End Class

End Namespace
