Imports System
Imports System.Collections.Generic
Imports MySql.Data.MySqlClient

Namespace SumyPortal

    ''' <summary>
    ''' Відгуки й рейтинг оголошень (п.9 уточненої постановки — "Рейтинги/відгуки").
    ''' Без модерації — публікуються одразу (свідоме MVP-спрощення). Без перевірки
    ''' факту замовлення — портал не веде трекінг угод (контакт по телефону поза
    ''' системою), тому "підтверджена покупка" неможлива; єдине обмеження — один
    ''' відгук від одного користувача на одне оголошення (UNIQUE у БД) і заборона
    ''' власнику оголошення відгукуватись на самого себе (перевіряється на сторінці).
    ''' </summary>
    Public Class Review
        Public Property ReviewId As Integer
        Public Property ServiceId As Integer
        Public Property ConsumerId As Integer
        Public Property ConsumerName As String
        Public Property Rating As Integer
        Public Property Comment As String
        Public Property CreatedAt As DateTime

        ''' <summary>Відповідь постачальника (п.19, наступна фіча понад MVP, 2026-09-12) —
        ''' одна на відгук, зберігається просто на цьому ж рядку (без окремої таблиці/треду).
        ''' Nothing — постачальник ще не відповідав.</summary>
        Public Property ProviderReply As String
        Public Property ProviderReplyAt As DateTime?

        Public Shared Function HasReviewed(serviceId As Integer, consumerId As Integer) As Boolean
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT COUNT(*) FROM Reviews WHERE ServiceId = @ServiceId AND ConsumerId = @ConsumerId;", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    cmd.Parameters.AddWithValue("@ConsumerId", consumerId)
                    Return Convert.ToInt32(cmd.ExecuteScalar()) > 0
                End Using
            End Using
        End Function

        ''' <summary>Повертає False, якщо відгук від цього користувача на це оголошення вже існує (UNIQUE у БД).</summary>
        Public Shared Function Add(serviceId As Integer, consumerId As Integer, rating As Integer, comment As String) As Boolean
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "INSERT IGNORE INTO Reviews (ServiceId, ConsumerId, Rating, Comment, CreatedAt) " &
                    "VALUES (@ServiceId, @ConsumerId, @Rating, @Comment, UTC_TIMESTAMP());", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    cmd.Parameters.AddWithValue("@ConsumerId", consumerId)
                    cmd.Parameters.AddWithValue("@Rating", rating)
                    cmd.Parameters.AddWithValue("@Comment", If(String.IsNullOrWhiteSpace(comment), DBNull.Value, CObj(comment.Trim())))
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        ''' <summary>Усі відгуки оголошення, найновіші зверху, з іменем автора.</summary>
        Public Shared Function GetByService(serviceId As Integer) As List(Of Review)
            Dim result As New List(Of Review)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT r.ReviewId, r.ServiceId, r.ConsumerId, u.FullName AS ConsumerName, " &
                    "r.Rating, r.Comment, r.CreatedAt, r.ProviderReply, r.ProviderReplyAt " &
                    "FROM Reviews r JOIN Users u ON u.UserId = r.ConsumerId " &
                    "WHERE r.ServiceId = @ServiceId ORDER BY r.CreatedAt DESC;", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            result.Add(New Review With {
                                .ReviewId = reader.GetInt32("ReviewId"),
                                .ServiceId = reader.GetInt32("ServiceId"),
                                .ConsumerId = reader.GetInt32("ConsumerId"),
                                .ConsumerName = reader.GetString("ConsumerName"),
                                .Rating = reader.GetInt32("Rating"),
                                .Comment = If(reader.IsDBNull(reader.GetOrdinal("Comment")), Nothing, reader.GetString("Comment")),
                                .CreatedAt = reader.GetDateTime("CreatedAt"),
                                .ProviderReply = If(reader.IsDBNull(reader.GetOrdinal("ProviderReply")), Nothing, reader.GetString("ProviderReply")),
                                .ProviderReplyAt = If(reader.IsDBNull(reader.GetOrdinal("ProviderReplyAt")), CType(Nothing, DateTime?), reader.GetDateTime("ProviderReplyAt"))
                            })
                        End While
                    End Using
                End Using
            End Using
            Return result
        End Function

        ''' <summary>Постачальник додає/редагує/прибирає (порожній текст) відповідь під відгуком
        ''' на власне оголошення. Власника перевіряє сам SQL (JOIN на Services.ProviderId) —
        ''' підроблений постбек на чужий відгук просто не оновить жодного рядка, той самий
        ''' прийом, що Service.Update для постачальника. Повертає False, якщо відгук не
        ''' належить оголошенню цього постачальника (або взагалі не існує).</summary>
        Public Shared Function SetProviderReply(reviewId As Integer, providerId As Integer, replyText As String) As Boolean
            Dim trimmed = If(String.IsNullOrWhiteSpace(replyText), Nothing, replyText.Trim())
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "UPDATE Reviews r JOIN Services s ON s.ServiceId = r.ServiceId " &
                    "SET r.ProviderReply = @Reply, r.ProviderReplyAt = " & If(trimmed Is Nothing, "NULL", "UTC_TIMESTAMP()") & " " &
                    "WHERE r.ReviewId = @ReviewId AND s.ProviderId = @ProviderId;", conn)
                    cmd.Parameters.AddWithValue("@Reply", If(trimmed Is Nothing, DBNull.Value, CObj(trimmed)))
                    cmd.Parameters.AddWithValue("@ReviewId", reviewId)
                    cmd.Parameters.AddWithValue("@ProviderId", providerId)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        ''' <summary>Середня оцінка (Nothing, якщо відгуків немає) і їх кількість — одним запитом.</summary>
        Public Shared Sub GetSummary(serviceId As Integer, ByRef average As Decimal?, ByRef count As Integer)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT AVG(Rating), COUNT(*) FROM Reviews WHERE ServiceId = @ServiceId;", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    Using reader = cmd.ExecuteReader()
                        reader.Read()
                        count = reader.GetInt32(1)
                        average = If(reader.IsDBNull(0), CType(Nothing, Decimal?), reader.GetDecimal(0))
                    End Using
                End Using
            End Using
        End Sub

        ''' <summary>Видалення відгуку адміном (п.12 уточненої постановки, 2026-09-11) — лише видалення,
        ''' без редагування чужого тексту (редагувати чиюсь думку від імені адміна нелогічно).</summary>
        Public Shared Function AdminDelete(reviewId As Integer) As Boolean
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand("DELETE FROM Reviews WHERE ReviewId = @ReviewId;", conn)
                    cmd.Parameters.AddWithValue("@ReviewId", reviewId)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

    End Class

End Namespace
