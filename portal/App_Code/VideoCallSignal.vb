Imports System
Imports System.Collections.Generic
Imports MySql.Data.MySqlClient

Namespace SumyPortal

    ''' <summary>
    ''' Сигналізація для WebRTC-відеодзвінків Постачальник↔Споживач (наступна
    ''' фіча понад MVP, обрано користувачем, 2026-09-13). Сама аудіо/відео-доріжка
    ''' йде напряму між браузерами (WebRTC P2P, STUN-сервер лише для обходу NAT) —
    ''' ця таблиця передає лише SDP offer/answer і ICE-кандидати, той самий принцип
    ''' AJAX-polling, що вже жива переписка (App_Code/MessagesPoll.vb). Розмова =
    ''' та сама пара (ServiceId, ConsumerId), що Messages/DialogMessage — дзвінок
    ''' прив'язаний до конкретної розмови навколо оголошення, не окрема сутність.
    ''' Клас навмисно НЕ "Signal" — надто загальне ім'я, ризик майбутнього
    ''' конфлікту (та сама обережність, що DialogMessage/ServiceReport, README).
    ''' </summary>
    Public Class VideoCallSignal
        Public Property SignalId As Integer
        Public Property SenderId As Integer
        Public Property SignalType As String
        Public Property Payload As String
        Public Property CreatedAt As DateTime

        ''' <summary>Записує сигнал. Перед вставкою прибирає застарілі сигнали цієї ж
        ''' розмови (старші за 2 хв) — легке "сміттєприбирання" без окремого throttle/
        ''' cron (на відміну від щоденних задач у Global.asax, тут природний момент
        ''' чистки — сам факт нового дзвінка/сигналу), захищає від необмеженого
        ''' накопичення покинутих спроб з'єднання.</summary>
        Public Shared Sub Send(serviceId As Integer, consumerId As Integer, senderId As Integer, signalType As String, payload As String)
            Using conn = DbHelper.GetConnection()
                Using cleanupCmd As New MySqlCommand(
                    "DELETE FROM VideoCallSignals WHERE ServiceId = @ServiceId AND ConsumerId = @ConsumerId " &
                    "AND CreatedAt < DATE_SUB(UTC_TIMESTAMP(), INTERVAL 2 MINUTE);", conn)
                    cleanupCmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    cleanupCmd.Parameters.AddWithValue("@ConsumerId", consumerId)
                    cleanupCmd.ExecuteNonQuery()
                End Using

                ' Hangup і новий offer означають "чиста дошка" — прибираємо все попереднє
                ' цієї розмови одразу (перед вставкою нового рядка нижче), а не чекаємо
                ' 2-хвилинний поріг вище: наступний дзвінок не повинен бачити сигнали
                ' попереднього (offer) чи лишати по собі сміття (hangup).
                If signalType = "hangup" OrElse signalType = "offer" Then
                    Using clearCmd As New MySqlCommand(
                        "DELETE FROM VideoCallSignals WHERE ServiceId = @ServiceId AND ConsumerId = @ConsumerId;", conn)
                        clearCmd.Parameters.AddWithValue("@ServiceId", serviceId)
                        clearCmd.Parameters.AddWithValue("@ConsumerId", consumerId)
                        clearCmd.ExecuteNonQuery()
                    End Using
                End If

                Using cmd As New MySqlCommand(
                    "INSERT INTO VideoCallSignals (ServiceId, ConsumerId, SenderId, SignalType, Payload, CreatedAt) " &
                    "VALUES (@ServiceId, @ConsumerId, @SenderId, @SignalType, @Payload, UTC_TIMESTAMP());", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    cmd.Parameters.AddWithValue("@ConsumerId", consumerId)
                    cmd.Parameters.AddWithValue("@SenderId", senderId)
                    cmd.Parameters.AddWithValue("@SignalType", signalType)
                    cmd.Parameters.AddWithValue("@Payload", payload)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        End Sub

        ''' <summary>Нові сигнали розмови після заданого SignalId — той самий принцип,
        ''' що DialogMessage.GetThreadAfter (afterId у клієнта, polling). Повертає сигнали
        ''' ВІД ІНШОЇ сторони (не власні, щоб клієнт не обробляв те, що сам щойно надіслав).</summary>
        Public Shared Function GetSignalsAfter(serviceId As Integer, consumerId As Integer, currentUserId As Integer, afterId As Integer) As List(Of VideoCallSignal)
            Dim result As New List(Of VideoCallSignal)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT SignalId, SenderId, SignalType, Payload, CreatedAt FROM VideoCallSignals " &
                    "WHERE ServiceId = @ServiceId AND ConsumerId = @ConsumerId AND SignalId > @AfterId AND SenderId <> @CurrentUserId " &
                    "ORDER BY SignalId ASC;", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    cmd.Parameters.AddWithValue("@ConsumerId", consumerId)
                    cmd.Parameters.AddWithValue("@AfterId", afterId)
                    cmd.Parameters.AddWithValue("@CurrentUserId", currentUserId)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            result.Add(New VideoCallSignal With {
                                .SignalId = reader.GetInt32("SignalId"),
                                .SenderId = reader.GetInt32("SenderId"),
                                .SignalType = reader.GetString("SignalType"),
                                .Payload = reader.GetString("Payload"),
                                .CreatedAt = reader.GetDateTime("CreatedAt")
                            })
                        End While
                    End Using
                End Using
            End Using
            Return result
        End Function

    End Class

End Namespace
