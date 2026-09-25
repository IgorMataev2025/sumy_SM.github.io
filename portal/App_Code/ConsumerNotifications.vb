Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports MySql.Data.MySqlClient

Namespace SumyPortal

    ''' <summary>
    ''' Налаштування листів споживача (2026-09-25, роль Споживач, п.5 і п.8 аудиту;
    ''' migration_022): персональний дайджест (категорії + район, або вимкнено) і
    ''' сповіщення про зміни в «Обраному». Окремий клас, а не поля UserAccount, щоб не
    ''' розширювати SelectColumns/Map, якими користуються всі сторінки.
    ''' </summary>
    Public Class NotificationSettings
        Public Property UserId As Integer
        Public Property DigestEnabled As Boolean = True
        ''' <summary>Nothing — усі райони.</summary>
        Public Property DigestDistrict As String
        ''' <summary>Порожньо — усі категорії.</summary>
        Public Property DigestCategoryIds As New HashSet(Of Integer)
        Public Property FavoriteAlertsEnabled As Boolean = True

        ''' <summary>Чи підходить нове оголошення під уподобання цього споживача. Оголошення
        ''' без району (District NULL — "по всій області") підходить під будь-який район.</summary>
        Public Function Matches(item As Service.DigestListing) As Boolean
            If DigestCategoryIds.Count > 0 AndAlso Not DigestCategoryIds.Contains(item.CategoryId) Then Return False
            If Not String.IsNullOrEmpty(DigestDistrict) AndAlso Not String.IsNullOrEmpty(item.District) AndAlso
               Not String.Equals(DigestDistrict, item.District, StringComparison.Ordinal) Then Return False
            Return True
        End Function

        Public Shared Function GetByUser(userId As Integer) As NotificationSettings
            Dim all = GetForUsers(New List(Of Integer) From {userId})
            Dim result As NotificationSettings = Nothing
            If all.TryGetValue(userId, result) Then Return result
            Return New NotificationSettings With {.UserId = userId}
        End Function

        ''' <summary>Налаштування кількох користувачів двома запитами (для розсилки дайджесту
        ''' — без окремого SELECT на кожного адресата).</summary>
        Public Shared Function GetForUsers(userIds As List(Of Integer)) As Dictionary(Of Integer, NotificationSettings)
            Dim result As New Dictionary(Of Integer, NotificationSettings)
            If userIds.Count = 0 Then Return result
            Dim idList = String.Join(",", userIds.Select(Function(id) id.ToString()))

            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT UserId, DigestEnabled, DigestDistrict, FavoriteAlertsEnabled FROM Users WHERE UserId IN (" & idList & ");", conn)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            result(reader.GetInt32("UserId")) = New NotificationSettings With {
                                .UserId = reader.GetInt32("UserId"),
                                .DigestEnabled = reader.GetBoolean("DigestEnabled"),
                                .DigestDistrict = If(reader.IsDBNull(reader.GetOrdinal("DigestDistrict")), Nothing, reader.GetString("DigestDistrict")),
                                .FavoriteAlertsEnabled = reader.GetBoolean("FavoriteAlertsEnabled")
                            }
                        End While
                    End Using
                End Using
                Using cmd As New MySqlCommand(
                    "SELECT UserId, CategoryId FROM UserDigestCategories WHERE UserId IN (" & idList & ");", conn)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim settings As NotificationSettings = Nothing
                            If result.TryGetValue(reader.GetInt32("UserId"), settings) Then settings.DigestCategoryIds.Add(reader.GetInt32("CategoryId"))
                        End While
                    End Using
                End Using
            End Using
            Return result
        End Function

        Public Shared Sub Save(settings As NotificationSettings)
            Using conn = DbHelper.GetConnection()
                Using tx = conn.BeginTransaction()
                    Using cmd As New MySqlCommand(
                        "UPDATE Users SET DigestEnabled = @DigestEnabled, DigestDistrict = @DigestDistrict, " &
                        "FavoriteAlertsEnabled = @FavoriteAlertsEnabled WHERE UserId = @UserId;", conn, tx)
                        cmd.Parameters.AddWithValue("@DigestEnabled", settings.DigestEnabled)
                        cmd.Parameters.AddWithValue("@DigestDistrict", If(String.IsNullOrEmpty(settings.DigestDistrict), DBNull.Value, CObj(settings.DigestDistrict)))
                        cmd.Parameters.AddWithValue("@FavoriteAlertsEnabled", settings.FavoriteAlertsEnabled)
                        cmd.Parameters.AddWithValue("@UserId", settings.UserId)
                        cmd.ExecuteNonQuery()
                    End Using
                    Using cmd As New MySqlCommand("DELETE FROM UserDigestCategories WHERE UserId = @UserId;", conn, tx)
                        cmd.Parameters.AddWithValue("@UserId", settings.UserId)
                        cmd.ExecuteNonQuery()
                    End Using
                    For Each categoryId In settings.DigestCategoryIds
                        Using cmd As New MySqlCommand(
                            "INSERT INTO UserDigestCategories (UserId, CategoryId) VALUES (@UserId, @CategoryId);", conn, tx)
                            cmd.Parameters.AddWithValue("@UserId", settings.UserId)
                            cmd.Parameters.AddWithValue("@CategoryId", categoryId)
                            cmd.ExecuteNonQuery()
                        End Using
                    Next
                    tx.Commit()
                End Using
            End Using
        End Sub
    End Class

    ''' <summary>
    ''' Листи про зміни в «Обраному» (2026-09-25, п.8 аудиту споживача): раз на добу
    ''' (Global.asax, той самий "poor man's cron", що дайджест) порівнюємо поточні
    ''' ціну/статус оголошення зі знімком у Favorites (LastKnownPrice/LastKnownStatus).
    ''' Лист — лише про те, що важливо споживачу: змінилась ціна, оголошення зняли з
    ''' публікації або повернули. Переходи між неопублікованими статусами (напр.
    ''' Draft → Pending) мовчки оновлюють знімок. Знімок оновлюється після перевірки
    ''' незалежно від того, чи дійшов лист (той самий підхід, що DigestSender).
    ''' </summary>
    Public NotInheritable Class FavoriteAlertSender

        Private Class Change
            Public Property UserId As Integer
            Public Property Email As String
            Public Property AlertsEnabled As Boolean
            Public Property ServiceId As Integer
            Public Property Title As String
            Public Property OldPrice As Decimal?
            Public Property NewPrice As Decimal?
            Public Property OldStatus As String
            Public Property NewStatus As String
        End Class

        Public Shared Function SendDue(baseUrl As String) As Integer
            Dim changes As New List(Of Change)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT f.UserId, u.Email, u.FavoriteAlertsEnabled, u.IsActive, u.EmailConfirmed, f.ServiceId, s.Title, " &
                    "f.LastKnownPrice, s.Price, f.LastKnownStatus, s.Status " &
                    "FROM Favorites f JOIN Services s ON s.ServiceId = f.ServiceId JOIN Users u ON u.UserId = f.UserId " &
                    "WHERE f.UserId <> s.ProviderId " &
                    "AND (NOT (f.LastKnownPrice <=> s.Price) OR NOT (f.LastKnownStatus <=> s.Status));", conn)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            changes.Add(New Change With {
                                .UserId = reader.GetInt32("UserId"),
                                .Email = reader.GetString("Email"),
                                .AlertsEnabled = reader.GetBoolean("FavoriteAlertsEnabled") AndAlso reader.GetBoolean("IsActive") AndAlso reader.GetBoolean("EmailConfirmed"),
                                .ServiceId = reader.GetInt32("ServiceId"),
                                .Title = reader.GetString("Title"),
                                .OldPrice = If(reader.IsDBNull(reader.GetOrdinal("LastKnownPrice")), CType(Nothing, Decimal?), reader.GetDecimal("LastKnownPrice")),
                                .NewPrice = If(reader.IsDBNull(reader.GetOrdinal("Price")), CType(Nothing, Decimal?), reader.GetDecimal("Price")),
                                .OldStatus = If(reader.IsDBNull(reader.GetOrdinal("LastKnownStatus")), Nothing, reader.GetString("LastKnownStatus")),
                                .NewStatus = reader.GetString("Status")
                            })
                        End While
                    End Using
                End Using
            End Using
            If changes.Count = 0 Then Return 0

            Dim sentCount = 0
            For Each group In changes.GroupBy(Function(c) c.UserId)
                Dim first = group.First()
                If first.AlertsEnabled Then
                    Dim lines = group.Select(Function(c) DescribeChange(c, baseUrl)).Where(Function(l) l IsNot Nothing).ToList()
                    If lines.Count > 0 Then
                        Try
                            EmailSender.Send(first.Email, "Зміни в оголошеннях з вашого «Обраного» — Safina", BuildBody(lines, baseUrl))
                            sentCount += 1
                        Catch
                            ' Помилка листа не повинна зупиняти розсилку іншим (як DigestSender).
                        End Try
                    End If
                End If
            Next

            ' Знімок — для всіх знайдених змін, зокрема тих, про які лист не потрібен
            ' (вимкнені сповіщення, неважливий перехід статусу), інакше вони б "висіли" щодня.
            Using conn = DbHelper.GetConnection()
                For Each c In changes
                    Using cmd As New MySqlCommand(
                        "UPDATE Favorites SET LastKnownPrice = @Price, LastKnownStatus = @Status " &
                        "WHERE UserId = @UserId AND ServiceId = @ServiceId;", conn)
                        cmd.Parameters.AddWithValue("@Price", If(c.NewPrice.HasValue, CObj(c.NewPrice.Value), DBNull.Value))
                        cmd.Parameters.AddWithValue("@Status", c.NewStatus)
                        cmd.Parameters.AddWithValue("@UserId", c.UserId)
                        cmd.Parameters.AddWithValue("@ServiceId", c.ServiceId)
                        cmd.ExecuteNonQuery()
                    End Using
                Next
            End Using
            Return sentCount
        End Function

        ''' <summary>Рядок листа або Nothing, якщо зміна споживачу нецікава.</summary>
        Private Shared Function DescribeChange(c As Change, baseUrl As String) As String
            Dim wasPublished = (c.OldStatus = "Approved")
            Dim isPublished = (c.NewStatus = "Approved")
            If wasPublished AndAlso Not isPublished Then
                Return String.Format("• «{0}» — знято з публікації", c.Title)
            End If
            If Not wasPublished AndAlso isPublished Then
                Return String.Format("• «{0}» — знову доступне: {1}/ServiceDetails.aspx?id={2}", c.Title, baseUrl, c.ServiceId)
            End If
            If isPublished AndAlso Not Nullable.Equals(c.OldPrice, c.NewPrice) Then
                Return String.Format("• «{0}» — ціна: {1} → {2}: {3}/ServiceDetails.aspx?id={4}",
                                     c.Title, FormatPrice(c.OldPrice), FormatPrice(c.NewPrice), baseUrl, c.ServiceId)
            End If
            Return Nothing
        End Function

        Private Shared Function FormatPrice(price As Decimal?) As String
            Return If(price.HasValue, price.Value.ToString("0.##") & " грн", "за домовленістю")
        End Function

        Private Shared Function BuildBody(lines As List(Of String), baseUrl As String) As String
            Dim sb As New StringBuilder()
            sb.AppendLine("Змінилися оголошення, які ви додали в «Обране» на порталі послуг Safina:")
            sb.AppendLine()
            For Each line In lines
                sb.AppendLine(line)
            Next
            sb.AppendLine()
            sb.AppendLine("Ваше «Обране»: " & baseUrl & "/Favorites.aspx")
            sb.AppendLine("Вимкнути ці листи можна в профілі: " & baseUrl & "/Profile.aspx")
            Return sb.ToString()
        End Function

    End Class

End Namespace
