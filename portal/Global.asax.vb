Imports System
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

            Dim days As Integer
            If Not Integer.TryParse(ConfigurationManager.AppSettings("StaleServiceDays"), days) Then days = 90

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
