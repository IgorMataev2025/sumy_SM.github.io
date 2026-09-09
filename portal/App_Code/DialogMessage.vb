Imports System
Imports System.Collections.Generic
Imports MySql.Data.MySqlClient

Namespace SumyPortal

    ''' <summary>Рядок списку розмов (Messages.aspx) — остання репліка + другий учасник.</summary>
    Public Class ConversationSummary
        Public Property ServiceId As Integer
        Public Property ServiceTitle As String
        Public Property ConsumerId As Integer
        Public Property OtherPartyName As String
        Public Property LastBody As String
        Public Property LastSentAt As DateTime
    End Class

    ''' <summary>
    ''' Одне повідомлення в розмові навколо оголошення + доступ до таблиці Messages
    ''' (п.10 уточненої постановки, 2026-09-09 — обмін інформацією
    ''' Постачальник↔Споживач). Клас навмисно НЕ називається "Message" —
    ''' у проєкті вже є параметр "message As String" (ShowInfo) у десятці
    ''' файлів, а VB.NET регістронезалежний до імен типів (та сама грабля, що
    ''' вже траплялась із Service/AccountType, README).
    '''
    ''' Розмова = пара (ServiceId, ConsumerId) — постачальник визначається через
    ''' Services.ProviderId, окремо не зберігається. MVP-спрощення (рішення
    ''' користувача): ініціює лише споживач (кнопка на ServiceDetails.aspx),
    ''' постачальник тільки відповідає в уже створеній розмові; без індикаторів
    ''' "непрочитано", без real-time — звичайний postback.
    ''' </summary>
    Public Class DialogMessage
        Public Property MessageId As Integer
        Public Property ServiceId As Integer
        Public Property ConsumerId As Integer
        Public Property SenderId As Integer
        Public Property SenderName As String
        Public Property Body As String
        Public Property SentAt As DateTime

        ''' <summary>Пише повідомлення в розмову. Доступ (чи має право писати саме ця людина)
        ''' перевіряється на сторінці до виклику — тут лише запис.</summary>
        Public Shared Sub Send(serviceId As Integer, consumerId As Integer, senderId As Integer, body As String)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "INSERT INTO Messages (ServiceId, ConsumerId, SenderId, Body, SentAt) " &
                    "VALUES (@ServiceId, @ConsumerId, @SenderId, @Body, UTC_TIMESTAMP());", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    cmd.Parameters.AddWithValue("@ConsumerId", consumerId)
                    cmd.Parameters.AddWithValue("@SenderId", senderId)
                    cmd.Parameters.AddWithValue("@Body", body.Trim())
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        End Sub

        ''' <summary>Уся переписка однієї розмови, найстаріші зверху (як лінійний чат).</summary>
        Public Shared Function GetThread(serviceId As Integer, consumerId As Integer) As List(Of DialogMessage)
            Dim result As New List(Of DialogMessage)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT m.MessageId, m.ServiceId, m.ConsumerId, m.SenderId, u.FullName AS SenderName, m.Body, m.SentAt " &
                    "FROM Messages m JOIN Users u ON u.UserId = m.SenderId " &
                    "WHERE m.ServiceId = @ServiceId AND m.ConsumerId = @ConsumerId " &
                    "ORDER BY m.SentAt ASC;", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    cmd.Parameters.AddWithValue("@ConsumerId", consumerId)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            result.Add(New DialogMessage With {
                                .MessageId = reader.GetInt32("MessageId"),
                                .ServiceId = reader.GetInt32("ServiceId"),
                                .ConsumerId = reader.GetInt32("ConsumerId"),
                                .SenderId = reader.GetInt32("SenderId"),
                                .SenderName = reader.GetString("SenderName"),
                                .Body = reader.GetString("Body"),
                                .SentAt = reader.GetDateTime("SentAt")
                            })
                        End While
                    End Using
                End Using
            End Using
            Return result
        End Function

        ''' <summary>Розмови споживача — по одній на кожне оголошення, з якого він написав.</summary>
        Public Shared Function GetConversationsForConsumer(consumerId As Integer) As List(Of ConversationSummary)
            Return QuerySummaries(
                "SELECT x.ServiceId, s.Title AS ServiceTitle, x.ConsumerId, p.FullName AS OtherPartyName, x.Body AS LastBody, x.SentAt AS LastSentAt " &
                "FROM (SELECT m.ServiceId, m.ConsumerId, m.Body, m.SentAt, " &
                "ROW_NUMBER() OVER (PARTITION BY m.ServiceId, m.ConsumerId ORDER BY m.SentAt DESC) AS rn " &
                "FROM Messages m WHERE m.ConsumerId = @UserId) x " &
                "JOIN Services s ON s.ServiceId = x.ServiceId " &
                "JOIN Users p ON p.UserId = s.ProviderId " &
                "WHERE x.rn = 1 ORDER BY x.SentAt DESC;", consumerId)
        End Function

        ''' <summary>Розмови постачальника — по одній на кожну пару (оголошення, споживач), що йому писали.</summary>
        Public Shared Function GetConversationsForProvider(providerId As Integer) As List(Of ConversationSummary)
            Return QuerySummaries(
                "SELECT x.ServiceId, s.Title AS ServiceTitle, x.ConsumerId, c.FullName AS OtherPartyName, x.Body AS LastBody, x.SentAt AS LastSentAt " &
                "FROM (SELECT m.ServiceId, m.ConsumerId, m.Body, m.SentAt, " &
                "ROW_NUMBER() OVER (PARTITION BY m.ServiceId, m.ConsumerId ORDER BY m.SentAt DESC) AS rn " &
                "FROM Messages m JOIN Services s2 ON s2.ServiceId = m.ServiceId WHERE s2.ProviderId = @UserId) x " &
                "JOIN Services s ON s.ServiceId = x.ServiceId " &
                "JOIN Users c ON c.UserId = x.ConsumerId " &
                "WHERE x.rn = 1 ORDER BY x.SentAt DESC;", providerId)
        End Function

        Private Shared Function QuerySummaries(sql As String, userId As Integer) As List(Of ConversationSummary)
            Dim result As New List(Of ConversationSummary)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@UserId", userId)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            result.Add(New ConversationSummary With {
                                .ServiceId = reader.GetInt32("ServiceId"),
                                .ServiceTitle = reader.GetString("ServiceTitle"),
                                .ConsumerId = reader.GetInt32("ConsumerId"),
                                .OtherPartyName = reader.GetString("OtherPartyName"),
                                .LastBody = reader.GetString("LastBody"),
                                .LastSentAt = reader.GetDateTime("LastSentAt")
                            })
                        End While
                    End Using
                End Using
            End Using
            Return result
        End Function

    End Class

End Namespace
