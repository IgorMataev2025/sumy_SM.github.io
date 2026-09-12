Imports System
Imports System.Collections.Generic
Imports MySql.Data.MySqlClient

Namespace SumyPortal

    ''' <summary>
    ''' Вподобані оголошення (п.8 уточненої постановки — "Вподобане/Збережене").
    ''' Доступно будь-якому залогіненому користувачу (Consumer і Provider), без
    ''' обмеження ролі — той самий підхід, що перегляд каталогу.
    ''' </summary>
    Public NotInheritable Class Favorite

        Public Shared Function IsFavorite(userId As Integer, serviceId As Integer) As Boolean
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT COUNT(*) FROM Favorites WHERE UserId = @UserId AND ServiceId = @ServiceId;", conn)
                    cmd.Parameters.AddWithValue("@UserId", userId)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    Return Convert.ToInt32(cmd.ExecuteScalar()) > 0
                End Using
            End Using
        End Function

        Public Shared Sub Add(userId As Integer, serviceId As Integer)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "INSERT IGNORE INTO Favorites (UserId, ServiceId, CreatedAt) " &
                    "VALUES (@UserId, @ServiceId, UTC_TIMESTAMP());", conn)
                    cmd.Parameters.AddWithValue("@UserId", userId)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        End Sub

        ''' <summary>Скільки разів це оголошення вподобали (MyServices.aspx, статистика для
        ''' постачальника, п.17) — незалежно від того, чи вподобане досі опубліковане.</summary>
        Public Shared Function GetCount(serviceId As Integer) As Integer
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand("SELECT COUNT(*) FROM Favorites WHERE ServiceId = @ServiceId;", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    Return Convert.ToInt32(cmd.ExecuteScalar())
                End Using
            End Using
        End Function

        Public Shared Sub Remove(userId As Integer, serviceId As Integer)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "DELETE FROM Favorites WHERE UserId = @UserId AND ServiceId = @ServiceId;", conn)
                    cmd.Parameters.AddWithValue("@UserId", userId)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        End Sub

        ''' <summary>Вподобані оголошення користувача, що досі опубліковані (Favorites.aspx) —
        ''' зняте з публікації чи видалене оголошення просто не показуємо, запис у
        ''' Favorites лишається "на випадок" повторної публікації.</summary>
        Public Shared Function GetByUser(userId As Integer) As List(Of Service)
            Dim result As New List(Of Service)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT s.ServiceId, s.ProviderId, s.CategoryId, c.Name AS CategoryName, s.Title, s.Description, " &
                    "s.Price, s.District, s.Phone, s.Latitude, s.Longitude, s.Status, s.RejectReason, s.CreatedAt, s.ViewCount, s.IsVerified " &
                    "FROM Favorites f " &
                    "JOIN Services s ON s.ServiceId = f.ServiceId " &
                    "JOIN Categories c ON c.CategoryId = s.CategoryId " &
                    "WHERE f.UserId = @UserId AND s.Status = 'Approved' " &
                    "ORDER BY f.CreatedAt DESC;", conn)
                    cmd.Parameters.AddWithValue("@UserId", userId)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            result.Add(Service.Map(reader))
                        End While
                    End Using
                End Using
            End Using
            Return result
        End Function

    End Class

End Namespace
