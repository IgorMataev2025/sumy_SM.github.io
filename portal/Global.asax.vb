Imports System
Imports System.Collections.Generic
Imports System.Configuration
Imports System.Web

Namespace SumyPortal

    Public Class [Global]
        Inherits HttpApplication

        ''' <summary>Автоматичне зняття застарілих оголошень (п.20, наступна фіча понад MVP,
        ''' 2026-09-12) — throttle у пам'яті процесу, щоб важка перевірка не бігла на
        ''' КОЖЕН запит. Скидається при рестарті пулу (не критично — гірше за це лише
        ''' пропущена перевірка на кілька годин, не втрата даних).</summary>
        Private Shared _nextStaleCheckAtUtc As DateTime = DateTime.MinValue
        Private Shared ReadOnly _staleCheckLock As New Object()

        ''' <summary>Дайджест на email (п.25, наступна фіча понад MVP, 2026-09-12) — той самий
        ''' throttle-прийом, що вище для застарілих оголошень, окрема пара змінних, бо
        ''' перевірки незалежні одна від одної.</summary>
        Private Shared _nextDigestCheckAtUtc As DateTime = DateTime.MinValue
        Private Shared ReadOnly _digestCheckLock As New Object()

        ''' <summary>Моніторинг залогінених користувачів у реальному часі (2026-09-16) —
        ''' той самий "poor man's cron" принцип, що вище, але per-user, а не один спільний
        ''' таймер: без цього UPDATE Users SET LastActivityAt писав би в БД на КОЖЕН
        ''' запит/AJAX-опитування (MessagesPoll.ashx/CallSignal.ashx кожні 3-5с). Скидається
        ''' при рестарті пулу — гірше за це лише коротка затримка появи в "онлайн", не
        ''' втрата даних.</summary>
        Private Shared ReadOnly _lastActivityWriteUtc As New Dictionary(Of String, DateTime)(StringComparer.OrdinalIgnoreCase)
        Private Shared ReadOnly _activityLock As New Object()

        Sub Application_Start(sender As Object, e As EventArgs)
            ' Ініціалізація на старті застосунку (кеші довідників тощо — пізніше).
        End Sub

        ''' <summary>
        ''' Голий корінь застосунку ("/") IIS резолвить у Default.aspx як default
        ''' document лише на стадії ResolveRequestCache — вона виконується ПІСЛЯ
        ''' AuthenticateRequest/AuthorizeRequest, де вже спрацьовує ASP.NET-
        ''' авторизація з Web.config. Тобто &lt;location path="Default.aspx"&gt;
        ''' (allow users="*", відкритий перегляд для USER, п.12 уточненої
        ''' постановки) до цього моменту ще не бачить, що запит насправді на
        ''' Default.aspx — Request.Path усе ще "/" — і анонімного відвідувача
        ''' редіректить на Login.aspx, хоча "/Default.aspx" напряму відкривався
        ''' нормально (знайдено живим тестом 2026-09-11). Application_BeginRequest
        ''' виконується найпершим у конвеєрі — переписуємо шлях тут, до того як
        ''' дійде до авторизації.
        ''' </summary>
        Sub Application_BeginRequest(sender As Object, e As EventArgs)
            Dim appPath = Request.ApplicationPath
            If String.IsNullOrEmpty(appPath) Then appPath = "/"
            If Not appPath.EndsWith("/") Then appPath &= "/"

            If String.Equals(Request.Path, appPath, StringComparison.OrdinalIgnoreCase) Then
                ' 3-аргументний overload (а не однорядковий RewritePath("~/Default.aspx"))
                ' — однорядковий мовчки ОЧИЩУЄ рядок запиту, коли в переданому шляху
                ' немає "?" (напр. https://site/?utm_source=... втратив би параметри).
                HttpContext.Current.RewritePath("~/Default.aspx", String.Empty, Request.QueryString.ToString())
            End If

            CheckStaleServicesOnce()
            CheckDigestOnce()
        End Sub

        ''' <summary>Автоматичне зняття застарілих оголошень (п.20, наступна фіча понад MVP,
        ''' 2026-09-12) — на shared-хостингу SmarterASP.NET немає окремого cron/планувальника,
        ''' тому перевірка "прив'язана" до звичайного трафіку сайту: раз на добу, throttle у
        ''' пам'яті (див. _nextStaleCheckAtUtc вище). Легкий спосіб імітувати планувальник без
        ''' зовнішньої інфраструктури — той самий підхід за духом, що вже Application_BeginRequest
        ''' використовується для фіксу голого домену (виконується найпершим у конвеєрі).
        ''' Service.ArchiveStaleApproved ідемпотентна (WHERE Status = 'Approved'), тож рідкісний
        ''' одночасний повторний запуск (гонка між двома запитами) не зашкодить, крім хіба
        ''' повторного листа — прийнятно для цього обсягу.</summary>
        Private Sub CheckStaleServicesOnce()
            If DateTime.UtcNow < _nextStaleCheckAtUtc Then Return

            SyncLock _staleCheckLock
                If DateTime.UtcNow < _nextStaleCheckAtUtc Then Return ' інший запит устиг першим, поки цей чекав на лок
                _nextStaleCheckAtUtc = DateTime.UtcNow.AddDays(1)
            End SyncLock

            Dim days = Service.StaleDays

            Dim archived = Service.ArchiveStaleApproved(days)
            For Each svc In archived
                Try
                    EmailSender.Send(svc.ProviderEmail, "Оголошення знято з публікації — Safina",
                        String.Format("Оголошення «{0}» автоматично знято з публікації, оскільки воно " &
                            "не оновлювалося понад {1} днів. Ви можете відредагувати його та повторно " &
                            "подати на модерацію в особистому кабінеті (розділ «Мої оголошення»).",
                            svc.Title, days))
                Catch
                    ' Помилка листа не повинна ламати саму архівацію — той самий підхід, що
                    ' NotifyOtherParty у MessageThread.aspx.vb.
                End Try
            Next

            ' Попередження — ПІСЛЯ архівації, щоб щойно зняте оголошення не отримало ще й
            ' лист «скоро знімемо» тим самим проходом.
            Dim myServicesUrl = Request.Url.GetLeftPart(UriPartial.Authority) & VirtualPathUtility.ToAbsolute("~/MyServices.aspx")
            For Each svc In Service.WarnExpiringApproved(days, Service.StaleWarningDays)
                Try
                    EmailSender.Send(svc.ProviderEmail, "Оголошення скоро буде знято з публікації — Safina",
                        String.Format("Оголошення «{0}» буде автоматично знято з публікації {1:dd.MM.yyyy}. " &
                            "Щоб воно лишалось у каталозі, натисніть «Продовжити» в розділі «Мої оголошення»: {2}",
                            svc.Title, svc.ExpiresAt.Value, myServicesUrl))
                Catch
                End Try
            Next
        End Sub

        ''' <summary>Дайджест на email (п.25, наступна фіча понад MVP, 2026-09-12) — раз на
        ''' добу перевіряємо, кому час надіслати (кожен користувач має власний "з якого
        ''' часу" — DigestSender.vb/UserAccount.GetDueForDigest), той самий "poor man's cron"
        ''' підхід, що вже CheckStaleServicesOnce вище. Request.Url доступний саме тут (а не
        ''' в DigestSender) — той самий прийом, що вже Sitemap.vb/Donate.aspx.vb для
        ''' самопідлаштування домену без хардкоду.</summary>
        Private Sub CheckDigestOnce()
            If DateTime.UtcNow < _nextDigestCheckAtUtc Then Return

            SyncLock _digestCheckLock
                If DateTime.UtcNow < _nextDigestCheckAtUtc Then Return
                _nextDigestCheckAtUtc = DateTime.UtcNow.AddDays(1)
            End SyncLock

            Dim days As Integer
            If Not Integer.TryParse(ConfigurationManager.AppSettings("DigestIntervalDays"), days) Then days = 7

            Dim baseUrl = Request.Url.GetLeftPart(UriPartial.Authority)
            DigestSender.SendDueDigests(days, baseUrl)

            ' Листи про зміни в «Обраному» (2026-09-25, migration_022) — та сама щоденна
            ' перевірка. Try — щоб збій тут не зачепив запит відвідувача, що її запустив.
            Try
                FavoriteAlertSender.SendDue(baseUrl)
            Catch
            End Try
        End Sub

        ''' <summary>Моніторинг залогінених користувачів у реальному часі (2026-09-16) —
        ''' PostAuthenticateRequest (не BeginRequest, як фікс голого домену вище) обрано
        ''' свідомо: це окрема стадія конвеєра, що виконується строго ПІСЛЯ
        ''' FormsAuthenticationModule, тож User.Identity тут уже гарантовано встановлений
        ''' з cookie (на BeginRequest автентифікація ще не відбулась).</summary>
        Sub Application_PostAuthenticateRequest(sender As Object, e As EventArgs)
            Dim user = HttpContext.Current.User
            If user Is Nothing OrElse user.Identity Is Nothing OrElse Not user.Identity.IsAuthenticated Then Return

            TrackActivityOnce(user.Identity.Name)
        End Sub

        Private Sub TrackActivityOnce(email As String)
            Dim throttleSeconds As Integer
            If Not Integer.TryParse(ConfigurationManager.AppSettings("ActivityWriteThrottleSeconds"), throttleSeconds) Then throttleSeconds = 60

            SyncLock _activityLock
                Dim lastWrite As DateTime
                If _lastActivityWriteUtc.TryGetValue(email, lastWrite) AndAlso DateTime.UtcNow < lastWrite.AddSeconds(throttleSeconds) Then
                    Return ' інший запит цього ж користувача вже оновив LastActivityAt нещодавно
                End If
                _lastActivityWriteUtc(email) = DateTime.UtcNow
            End SyncLock

            UserAccount.TouchLastActivity(email)
        End Sub

        Sub Session_Start(sender As Object, e As EventArgs)
        End Sub

        Sub Application_Error(sender As Object, e As EventArgs)
            ' TODO: логування помилок (файл/БД) перед виходом на бойовий хостинг.
        End Sub

        Sub Session_End(sender As Object, e As EventArgs)
        End Sub

        Sub Application_End(sender As Object, e As EventArgs)
        End Sub

    End Class

End Namespace
