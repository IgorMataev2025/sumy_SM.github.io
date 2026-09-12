Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace SumyPortal

    ''' <summary>
    ''' Дайджест на email (п.25, наступна фіча понад MVP, 2026-09-12) — періодичний лист
    ''' активним Споживачам зі списком нових Approved-оголошень від часу попереднього
    ''' дайджесту (чи від реєстрації, якщо дайджест ще не надсилався). Не персоналізовано
    ''' за категоріями/уподобаннями (свідоме MVP-спрощення, без нової сутності "підписка") —
    ''' один спільний список нових оголошень на всіх, лише "з якого часу" різне для кожного
    ''' користувача. Постачальники дайджест не отримують. Викликається з Global.asax
    ''' (CheckDigestOnce) — той самий "poor man's cron" throttle, що вже для автозняття
    ''' застарілих оголошень (п.20).
    ''' </summary>
    Public NotInheritable Class DigestSender

        ''' <summary>baseUrl — той самий прийом, що вже Sitemap.vb/Donate.aspx.vb: береться з
        ''' Request.Url.GetLeftPart(UriPartial.Authority) у Global.asax (де є доступ до
        ''' поточного запиту), а не хардкодиться тут — саме собою підлаштовується під будь-
        ''' який домен, без ручної правки коду при купівлі власного домену. Повертає
        ''' кількість реально надісланих листів (для діагностики/логів, Global.asax зараз
        ''' не використовує).</summary>
        Public Shared Function SendDueDigests(days As Integer, baseUrl As String) As Integer
            Dim dueUsers = UserAccount.GetDueForDigest(days)
            If dueUsers.Count = 0 Then Return 0

            Dim allApproved = Service.GetAllApprovedForDigest()
            Dim sentCount As Integer = 0

            For Each user In dueUsers
                Dim sinceUtc = If(user.LastDigestSentAt.HasValue, user.LastDigestSentAt.Value, user.CreatedAt)
                Dim newItems = allApproved.Where(Function(i) i.ApprovedAt > sinceUtc).ToList()

                ' Немає нових оголошень з минулого разу — не слати порожній лист і НЕ
                ' оновлювати LastDigestSentAt: користувач лишається "due", наступна щоденна
                ' перевірка (Global.asax) спробує знову, і як тільки з'явиться щось нове —
                ' лист піде одразу з усім накопиченим від sinceUtc.
                If newItems.Count = 0 Then Continue For

                Try
                    EmailSender.Send(user.Email, "Нові оголошення на порталі послуг Safina", BuildBody(newItems, baseUrl))
                    sentCount += 1
                Catch
                    ' Помилка листа не повинна ламати розсилку решті адресатів — той самий
                    ' підхід, що вже CheckStaleServicesOnce (Global.asax, п.20).
                End Try

                ' Оновлюємо позначку незалежно від того, чи лист реально дійшов (той самий
                ' підхід, що вже в проєкті для email-помилок) — інакше постійна помилка
                ' SMTP спричинила б нескінченні повторні спроби на кожній щоденній перевірці.
                UserAccount.MarkDigestSent(user.UserId)
            Next

            Return sentCount
        End Function

        Private Shared Function BuildBody(items As List(Of Service.DigestListing), baseUrl As String) As String
            Dim sb As New StringBuilder()
            sb.AppendLine("На порталі послуг Safina з'явились нові оголошення:")
            sb.AppendLine()
            For Each item In items
                sb.AppendLine(String.Format("• {0} ({1})", item.Title, item.CategoryName))
            Next
            sb.AppendLine()
            sb.AppendLine("Переглянути каталог: " & baseUrl & "/Catalog.aspx")
            Return sb.ToString()
        End Function

    End Class

End Namespace
