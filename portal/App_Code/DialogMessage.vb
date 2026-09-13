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

        ''' <summary>Лічильник непрочитаних (наступна фіча понад MVP, обрано автономно
        ''' циклом /loop, 2026-09-13) — повідомлення від іншої сторони, новіші за
        ''' MessageReadStatus.LastReadMessageId цього користувача в цій розмові.</summary>
        Public Property UnreadCount As Integer
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

        ''' <summary>Нові повідомлення розмови після заданого MessageId (найстаріші зверху) —
        ''' для "живої" переписки (наступна фіча понад MVP, обрано автономно циклом /loop,
        ''' 2026-09-13): MessagesPoll.ashx періодично питає лише те, чого клієнт ще не бачив,
        ''' замість перезавантаження всієї сторінки. Простий AJAX-polling, без WebSocket/
        ''' SignalR — той самий підхід, що вже "poor man's cron" у Global.asax (п.20/п.25):
        ''' shared-хостинг (SmarterASP.NET) навряд чи довгостроково підтримає постійні
        ''' з'єднання. Той самий IX_Messages_Thread (ServiceId, ConsumerId, SentAt) індекс
        ''' застосовний і тут — фільтр за ServiceId/ConsumerId залишається селективним.</summary>
        Public Shared Function GetThreadAfter(serviceId As Integer, consumerId As Integer, afterId As Integer) As List(Of DialogMessage)
            Dim result As New List(Of DialogMessage)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT m.MessageId, m.ServiceId, m.ConsumerId, m.SenderId, u.FullName AS SenderName, m.Body, m.SentAt " &
                    "FROM Messages m JOIN Users u ON u.UserId = m.SenderId " &
                    "WHERE m.ServiceId = @ServiceId AND m.ConsumerId = @ConsumerId AND m.MessageId > @AfterId " &
                    "ORDER BY m.MessageId ASC;", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    cmd.Parameters.AddWithValue("@ConsumerId", consumerId)
                    cmd.Parameters.AddWithValue("@AfterId", afterId)
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

        ''' <summary>Непрочитані цим користувачем повідомлення саме цієї розмови — той самий
        ''' підзапит підставляється в обидва QuerySummaries нижче (наступна фіча понад MVP,
        ''' обрано автономно циклом /loop, 2026-09-13). MessageReadStatus може не мати рядка
        ''' для цієї пари (розмову ще ніколи не відкривали) — COALESCE(...,0) тоді означає
        ''' "усе від інших непрочитане".</summary>
        Private Const UnreadCountSubquery As String =
            "(SELECT COUNT(*) FROM Messages m2 WHERE m2.ServiceId = x.ServiceId AND m2.ConsumerId = x.ConsumerId " &
            "AND m2.SenderId <> @UserId AND m2.MessageId > COALESCE(" &
            "(SELECT r.LastReadMessageId FROM MessageReadStatus r WHERE r.ServiceId = x.ServiceId AND r.ConsumerId = x.ConsumerId AND r.UserId = @UserId), 0)) AS UnreadCount "

        ''' <summary>Розмови споживача — по одній на кожне оголошення, з якого він написав.</summary>
        Public Shared Function GetConversationsForConsumer(consumerId As Integer) As List(Of ConversationSummary)
            Return QuerySummaries(
                "SELECT x.ServiceId, s.Title AS ServiceTitle, x.ConsumerId, p.FullName AS OtherPartyName, x.Body AS LastBody, x.SentAt AS LastSentAt, " &
                UnreadCountSubquery &
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
                "SELECT x.ServiceId, s.Title AS ServiceTitle, x.ConsumerId, c.FullName AS OtherPartyName, x.Body AS LastBody, x.SentAt AS LastSentAt, " &
                UnreadCountSubquery &
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
                                .LastSentAt = reader.GetDateTime("LastSentAt"),
                                .UnreadCount = reader.GetInt32("UnreadCount")
                            })
                        End While
                    End Using
                End Using
            End Using
            Return result
        End Function

        ''' <summary>Позначає розмову прочитаною цим користувачем "до" останнього наявного
        ''' повідомлення на момент виклику (наступна фіча понад MVP, обрано автономно циклом
        ''' /loop, 2026-09-13) — викликається з MessageThread.aspx.vb при кожному не-постбек
        ''' відкритті розмови. INSERT ... ON DUPLICATE KEY UPDATE — один запит замість
        ''' SELECT-потім-INSERT/UPDATE (той самий принцип, що Review.Add: INSERT IGNORE).
        ''' Якщо повідомлень ще немає (щойно відкрита порожня розмова) — LastReadMessageId
        ''' лишається 0, що безпечно (COALESCE у UnreadCountSubquery і так дає 0 за замовчуванням).</summary>
        Public Shared Sub MarkThreadAsRead(serviceId As Integer, consumerId As Integer, userId As Integer)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "INSERT INTO MessageReadStatus (ServiceId, ConsumerId, UserId, LastReadMessageId) " &
                    "SELECT @ServiceId, @ConsumerId, @UserId, COALESCE(MAX(MessageId), 0) FROM Messages " &
                    "WHERE ServiceId = @ServiceId AND ConsumerId = @ConsumerId " &
                    "ON DUPLICATE KEY UPDATE LastReadMessageId = VALUES(LastReadMessageId);", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    cmd.Parameters.AddWithValue("@ConsumerId", consumerId)
                    cmd.Parameters.AddWithValue("@UserId", userId)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        End Sub

        ''' <summary>Загальний лічильник непрочитаних повідомлень користувача (наступна фіча
        ''' понад MVP, обрано автономно циклом /loop, 2026-09-13) — по всіх розмовах одразу
        ''' (і як споживача, і як постачальника — той самий OR, що вже CallSignal.vb/
        ''' MessageThread.aspx.vb для перевірки участі), для бейджа "Повідомлення (N)" у
        ''' Site.master. LEFT JOIN — розмову ще могли жодного разу не відкривати.</summary>
        Public Shared Function GetUnreadCountForUser(userId As Integer) As Integer
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT COUNT(*) FROM Messages m " &
                    "JOIN Services s ON s.ServiceId = m.ServiceId " &
                    "LEFT JOIN MessageReadStatus r ON r.ServiceId = m.ServiceId AND r.ConsumerId = m.ConsumerId AND r.UserId = @UserId " &
                    "WHERE (m.ConsumerId = @UserId OR s.ProviderId = @UserId) " &
                    "AND m.SenderId <> @UserId " &
                    "AND m.MessageId > COALESCE(r.LastReadMessageId, 0);", conn)
                    cmd.Parameters.AddWithValue("@UserId", userId)
                    Return Convert.ToInt32(cmd.ExecuteScalar())
                End Using
            End Using
        End Function

    End Class

End Namespace
