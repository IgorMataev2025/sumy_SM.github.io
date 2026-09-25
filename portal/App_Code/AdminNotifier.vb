Imports System
Imports System.Collections.Generic
Imports System.Web
Imports MySql.Data.MySqlClient

Namespace SumyPortal

    ''' <summary>
    ''' Листи адміністраторам про нову роботу (2026-09-25, аудит Адміна, п.2): оголошення
    ''' подано на модерацію, надійшла скарга. Одразу, а не зведенням — обсяги поки малі,
    ''' а швидка модерація важлива для постачальника. Адресати — усі активні IsAdmin.
    ''' Помилка листа ніколи не ламає дію користувача (той самий принцип, що
    ''' AdminModeration.NotifyProvider).
    ''' </summary>
    Public NotInheritable Class AdminNotifier

        Public Shared Sub NewPendingService(serviceId As Integer, title As String)
            Send("Нове оголошення на модерації — Safina",
                 String.Format("Постачальник подав на модерацію оголошення «{0}».{1}{1}Черга модерації: {2}/AdminModeration.aspx",
                               title, Environment.NewLine, BaseUrl()))
        End Sub

        Public Shared Sub NewReport(serviceId As Integer, title As String, reasonLabel As String, comment As String)
            Send("Нова скарга на оголошення — Safina",
                 String.Format("Надійшла скарга на оголошення «{0}».{1}Причина: {2}{1}{3}{1}Скарги: {4}/AdminReports.aspx",
                               title, Environment.NewLine, reasonLabel,
                               If(String.IsNullOrWhiteSpace(comment), "", "Коментар: " & comment.Trim() & Environment.NewLine),
                               BaseUrl()))
        End Sub

        Private Shared Sub Send(subject As String, body As String)
            Try
                For Each email In GetAdminEmails()
                    Try
                        EmailSender.Send(email, subject, body)
                    Catch
                    End Try
                Next
            Catch
            End Try
        End Sub

        Private Shared Function GetAdminEmails() As List(Of String)
            Dim result As New List(Of String)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand("SELECT Email FROM Users WHERE IsAdmin = TRUE AND IsActive = TRUE;", conn)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            result.Add(reader.GetString("Email"))
                        End While
                    End Using
                End Using
            End Using
            Return result
        End Function

        Private Shared Function BaseUrl() As String
            Dim ctx = HttpContext.Current
            Return If(ctx IsNot Nothing, ctx.Request.Url.GetLeftPart(UriPartial.Authority), "")
        End Function

    End Class

End Namespace
