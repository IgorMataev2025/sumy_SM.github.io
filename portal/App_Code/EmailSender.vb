Imports System
Imports System.Configuration
Imports System.IO
Imports System.Net.Mail
Imports System.Web.Hosting

Namespace SumyPortal

    ''' <summary>
    ''' Відправка email-сповіщень (п.7 уточненої постановки — сповіщення при
    ''' модерації). Реального SMTP поки немає — Web.config appSettings
    ''' EmailDeliveryMethod=Pickup перемикає System.Net.Mail.SmtpClient на
    ''' SpecifiedPickupDirectory: листи пишуться як .eml-файли в
    ''' App_Data/EmailOutbox/ замість реальної відправки (штатний механізм
    ''' .NET, без сторонніх бібліотек — той самий dev-режим за духом, що вже
    ''' є для посилань підтвердження email/скидання пароля, тільки на рівні
    ''' System.Net.Mail, а не показу на екрані, бо тут нема кому показувати —
    ''' лист іде постачальнику, а не тому, хто зараз на сторінці).
    ''' На бойовому хостингу: прибрати EmailDeliveryMethod із Web.config і
    ''' додати реальні SMTP-налаштування в &lt;system.net&gt;&lt;mailSettings&gt;
    ''' — цей код лишається без змін.
    ''' </summary>
    Public NotInheritable Class EmailSender

        Public Shared Sub Send(toEmail As String, subject As String, body As String)
            Using message As New MailMessage()
                message.To.Add(toEmail)
                message.From = New MailAddress("noreply@safina-portal.com", "Портал послуг Safina")
                message.Subject = subject
                message.Body = body

                Using client As New SmtpClient()
                    If ConfigurationManager.AppSettings("EmailDeliveryMethod") = "Pickup" Then
                        Dim pickupDir = HostingEnvironment.MapPath("~/App_Data/EmailOutbox")
                        If Not Directory.Exists(pickupDir) Then Directory.CreateDirectory(pickupDir)
                        client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory
                        client.PickupDirectoryLocation = pickupDir
                    End If
                    ' Інакше — доставка й налаштування SMTP-сервера беруться зі
                    ' стандартної секції Web.config <system.net><mailSettings> (продакшн).

                    client.Send(message)
                End Using
            End Using
        End Sub

    End Class

End Namespace
