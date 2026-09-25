Imports System
Imports System.Collections.Generic
Imports MySql.Data.MySqlClient

Namespace SumyPortal

    ''' <summary>Донат LiqPay для адміна (2026-09-25, аудит Адміна, п.6).</summary>
    Public Class AdminDonation
        Public Property CreatedAt As DateTime
        Public Property Amount As Decimal
        Public Property Currency As String
        Public Property Status As String
        Public Property DonorName As String
        Public Property DonorEmail As String
        Public Property OrderId As String

        Public ReadOnly Property StatusLabel As String
            Get
                Select Case Status
                    Case "Success" : Return "Оплачено"
                    Case "Failure" : Return "Не вдалося"
                    Case Else : Return "Очікує"
                End Select
            End Get
        End Property
    End Class

    ''' <summary>Відгук у загальному списку адміна (2026-09-25, аудит Адміна, п.7).</summary>
    Public Class AdminReviewRow
        Public Property ReviewId As Integer
        Public Property ServiceId As Integer
        Public Property ServiceTitle As String
        Public Property ConsumerName As String
        Public Property ConsumerEmail As String
        Public Property Rating As Integer
        Public Property Comment As String
        Public Property CreatedAt As DateTime
    End Class

    ''' <summary>Показник за період для панелі (2026-09-25, аудит Адміна, п.8).</summary>
    Public Class AdminStatRow
        Public Property Label As String
        Public Property Last7 As String
        Public Property Last30 As String
        Public Property Total As String
    End Class

    ''' <summary>
    ''' Запити лише для адмін-сторінок (2026-09-25, аудит Адміна, п.6–8): донати, загальний
    ''' список відгуків, статистика за 7/30 днів. Окремо від доменних класів, бо це
    ''' звіти "навскрізь" кількох таблиць, а не поведінка однієї сутності.
    ''' </summary>
    Public NotInheritable Class AdminData

        Public Shared Function GetDonations(status As String, page As Integer, pageSize As Integer, ByRef total As Integer) As List(Of AdminDonation)
            Dim where = If(String.IsNullOrEmpty(status), "", " WHERE Status = @Status")
            Dim result As New List(Of AdminDonation)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand("SELECT COUNT(*) FROM Donations" & where & ";", conn)
                    If where <> "" Then cmd.Parameters.AddWithValue("@Status", status)
                    total = Convert.ToInt32(cmd.ExecuteScalar())
                End Using
                Using cmd As New MySqlCommand(
                    "SELECT CreatedAt, Amount, Currency, Status, DonorName, DonorEmail, OrderId FROM Donations" & where &
                    " ORDER BY CreatedAt DESC LIMIT @Limit OFFSET @Offset;", conn)
                    If where <> "" Then cmd.Parameters.AddWithValue("@Status", status)
                    cmd.Parameters.AddWithValue("@Limit", pageSize)
                    cmd.Parameters.AddWithValue("@Offset", (Math.Max(1, page) - 1) * pageSize)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            result.Add(New AdminDonation With {
                                .CreatedAt = reader.GetDateTime("CreatedAt"),
                                .Amount = reader.GetDecimal("Amount"),
                                .Currency = reader.GetString("Currency"),
                                .Status = reader.GetString("Status"),
                                .DonorName = If(reader.IsDBNull(reader.GetOrdinal("DonorName")), Nothing, reader.GetString("DonorName")),
                                .DonorEmail = If(reader.IsDBNull(reader.GetOrdinal("DonorEmail")), Nothing, reader.GetString("DonorEmail")),
                                .OrderId = reader.GetString("OrderId")
                            })
                        End While
                    End Using
                End Using
            End Using
            Return result
        End Function

        ''' <summary>Сума успішних донатів за останні days днів (Nothing — за весь час).</summary>
        Public Shared Function DonationsSum(days As Integer?) As Decimal
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT COALESCE(SUM(Amount), 0) FROM Donations WHERE Status = 'Success'" &
                    If(days.HasValue, " AND CreatedAt >= DATE_SUB(UTC_TIMESTAMP(), INTERVAL @Days DAY)", "") & ";", conn)
                    If days.HasValue Then cmd.Parameters.AddWithValue("@Days", days.Value)
                    Return Convert.ToDecimal(cmd.ExecuteScalar())
                End Using
            End Using
        End Function

        Public Shared Function GetRecentReviews(maxRating As Integer?, page As Integer, pageSize As Integer, ByRef total As Integer) As List(Of AdminReviewRow)
            Dim where = If(maxRating.HasValue, " WHERE r.Rating <= @MaxRating", "")
            Dim result As New List(Of AdminReviewRow)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand("SELECT COUNT(*) FROM Reviews r" & where & ";", conn)
                    If maxRating.HasValue Then cmd.Parameters.AddWithValue("@MaxRating", maxRating.Value)
                    total = Convert.ToInt32(cmd.ExecuteScalar())
                End Using
                Using cmd As New MySqlCommand(
                    "SELECT r.ReviewId, r.ServiceId, s.Title AS ServiceTitle, u.FullName AS ConsumerName, u.Email AS ConsumerEmail, " &
                    "r.Rating, r.Comment, r.CreatedAt " &
                    "FROM Reviews r JOIN Services s ON s.ServiceId = r.ServiceId JOIN Users u ON u.UserId = r.ConsumerId" & where &
                    " ORDER BY r.CreatedAt DESC LIMIT @Limit OFFSET @Offset;", conn)
                    If maxRating.HasValue Then cmd.Parameters.AddWithValue("@MaxRating", maxRating.Value)
                    cmd.Parameters.AddWithValue("@Limit", pageSize)
                    cmd.Parameters.AddWithValue("@Offset", (Math.Max(1, page) - 1) * pageSize)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            result.Add(New AdminReviewRow With {
                                .ReviewId = reader.GetInt32("ReviewId"),
                                .ServiceId = reader.GetInt32("ServiceId"),
                                .ServiceTitle = reader.GetString("ServiceTitle"),
                                .ConsumerName = reader.GetString("ConsumerName"),
                                .ConsumerEmail = reader.GetString("ConsumerEmail"),
                                .Rating = reader.GetInt32("Rating"),
                                .Comment = If(reader.IsDBNull(reader.GetOrdinal("Comment")), Nothing, reader.GetString("Comment")),
                                .CreatedAt = reader.GetDateTime("CreatedAt")
                            })
                        End While
                    End Using
                End Using
            End Using
            Return result
        End Function

        ''' <summary>Динаміка за 7 і 30 днів + усього — одним з'єднанням, по запиту на показник.</summary>
        Public Shared Function GetStats() As List(Of AdminStatRow)
            Dim rows As New List(Of AdminStatRow)
            Using conn = DbHelper.GetConnection()
                Dim count = Function(sql As String, days As Integer?) As Integer
                                Using cmd As New MySqlCommand(sql.Replace("{PERIOD}",
                                        If(days.HasValue, "DATE_SUB(UTC_TIMESTAMP(), INTERVAL " & days.Value & " DAY)", "'1970-01-01'")), conn)
                                    Return Convert.ToInt32(cmd.ExecuteScalar())
                                End Using
                            End Function
                Dim add = Sub(label As String, sql As String)
                              rows.Add(New AdminStatRow With {
                                  .Label = label,
                                  .Last7 = count(sql, 7).ToString(),
                                  .Last30 = count(sql, 30).ToString(),
                                  .Total = count(sql, Nothing).ToString()
                              })
                          End Sub

                add("Нові споживачі", "SELECT COUNT(*) FROM Users WHERE UserType = 'Consumer' AND CreatedAt >= {PERIOD};")
                add("Нові постачальники", "SELECT COUNT(*) FROM Users WHERE UserType = 'Provider' AND CreatedAt >= {PERIOD};")
                add("Нові оголошення", "SELECT COUNT(*) FROM Services WHERE CreatedAt >= {PERIOD};")
                add("Опубліковано оголошень", "SELECT COUNT(*) FROM ModerationLog WHERE Action = 'Approved' AND ActionDate >= {PERIOD};")
                add("Нові звернення (розмови)",
                    "SELECT COUNT(*) FROM (SELECT ServiceId, ConsumerId, MIN(SentAt) AS FirstAt FROM Messages GROUP BY ServiceId, ConsumerId) t WHERE t.FirstAt >= {PERIOD};")
                add("Повідомлень", "SELECT COUNT(*) FROM Messages WHERE SentAt >= {PERIOD};")
                add("Відгуків", "SELECT COUNT(*) FROM Reviews WHERE CreatedAt >= {PERIOD};")
                add("Скарг", "SELECT COUNT(*) FROM ServiceReports WHERE CreatedAt >= {PERIOD};")
            End Using
            rows.Add(New AdminStatRow With {
                .Label = "Донати, грн (оплачені)",
                .Last7 = DonationsSum(7).ToString("0.##"),
                .Last30 = DonationsSum(30).ToString("0.##"),
                .Total = DonationsSum(Nothing).ToString("0.##")
            })
            Return rows
        End Function

    End Class

End Namespace
