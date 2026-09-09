Imports System.Collections.Generic
Imports MySql.Data.MySqlClient

Namespace SumyPortal

    ''' <summary>
    ''' Журнал дій адміністратора (п.10 уточненої постановки, 2026-09-09).
    ''' Модерація оголошень уже пише в окрему таблицю ModerationLog (схема з
    ''' коробки, App_Code/Service.vb: Approve/Reject) — не чіпаємо робочий код.
    ''' Ця таблиця (AdminActionLog) покриває решту адмінських дій (блокування
    ''' користувача, зміни категорій). AdminLog.aspx показує обидва джерела
    ''' одним об'єднаним списком, відсортованим за датою.
    ''' </summary>
    Public Class AdminLogEntry
        Public Property ActionDate As DateTime
        Public Property AdminName As String
        Public Property Action As String
        Public Property Details As String
    End Class

    Public NotInheritable Class AdminActionLog

        ''' <summary>Пише запис у AdminActionLog. Ціль (email користувача, назва категорії) —
        ''' знімок тексту на момент дії, а не посилання: після знеособлення акаунта
        ''' (Profile.aspx, самовидалення) email у Users зміниться, а історія дій — ні.</summary>
        Public Shared Sub Log(adminId As Integer, action As String, targetDescription As String)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "INSERT INTO AdminActionLog (AdminId, Action, TargetDescription, ActionDate) " &
                    "VALUES (@AdminId, @Action, @TargetDescription, UTC_TIMESTAMP());", conn)
                    cmd.Parameters.AddWithValue("@AdminId", adminId)
                    cmd.Parameters.AddWithValue("@Action", action)
                    cmd.Parameters.AddWithValue("@TargetDescription", targetDescription)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        End Sub

        ''' <summary>Об'єднана стрічка AdminActionLog + ModerationLog, найновіші перші, обмежено `limit`.</summary>
        Public Shared Function GetRecent(Optional limit As Integer = 200) As List(Of AdminLogEntry)
            Dim result As New List(Of AdminLogEntry)

            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT l.ActionDate, u.Email AS AdminEmail, l.Action, l.TargetDescription " &
                    "FROM AdminActionLog l JOIN Users u ON u.UserId = l.AdminId " &
                    "ORDER BY l.ActionDate DESC LIMIT @Limit;", conn)
                    cmd.Parameters.AddWithValue("@Limit", limit)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            result.Add(New AdminLogEntry With {
                                .ActionDate = reader.GetDateTime("ActionDate"),
                                .AdminName = reader.GetString("AdminEmail"),
                                .Action = reader.GetString("Action"),
                                .Details = reader.GetString("TargetDescription")
                            })
                        End While
                    End Using
                End Using

                Using cmd As New MySqlCommand(
                    "SELECT m.ActionDate, u.Email AS AdminEmail, m.Action, m.Comment, s.Title AS ServiceTitle " &
                    "FROM ModerationLog m JOIN Users u ON u.UserId = m.AdminId " &
                    "JOIN Services s ON s.ServiceId = m.ServiceId " &
                    "ORDER BY m.ActionDate DESC LIMIT @Limit;", conn)
                    cmd.Parameters.AddWithValue("@Limit", limit)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim action = reader.GetString("Action")
                            Dim actionText = If(action = "Approved", "Схвалив оголошення", "Відхилив оголошення")
                            Dim comment = If(reader.IsDBNull(reader.GetOrdinal("Comment")), Nothing, reader.GetString("Comment"))
                            Dim details = """" & reader.GetString("ServiceTitle") & """"
                            If Not String.IsNullOrEmpty(comment) Then
                                details &= " — причина: " & comment
                            End If

                            result.Add(New AdminLogEntry With {
                                .ActionDate = reader.GetDateTime("ActionDate"),
                                .AdminName = reader.GetString("AdminEmail"),
                                .Action = actionText,
                                .Details = details
                            })
                        End While
                    End Using
                End Using
            End Using

            result.Sort(Function(a, b) b.ActionDate.CompareTo(a.ActionDate))
            If result.Count > limit Then result = result.GetRange(0, limit)
            Return result
        End Function

    End Class

End Namespace
