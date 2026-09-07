Imports System
Imports MySql.Data.MySqlClient

Namespace SumyPortal

    ''' <summary>
    ''' Донат на підтримку проєкту через LiqPay (п.1 уточненої постановки) +
    ''' доступ до таблиці Donations. Лише параметризовані запити (як і решта
    ''' моделей, ТЗ розділ 8).
    ''' </summary>
    Public Class Donation
        Public Property DonationId As Integer
        Public Property OrderId As String
        Public Property Amount As Decimal
        Public Property Currency As String
        Public Property Status As String
        Public Property LiqPayStatus As String
        Public Property PaymentId As String
        Public Property DonorName As String
        Public Property DonorEmail As String
        Public Property CreatedAt As DateTime

        Private Shared Function Map(reader As MySqlDataReader) As Donation
            Return New Donation With {
                .DonationId = reader.GetInt32("DonationId"),
                .OrderId = reader.GetString("OrderId"),
                .Amount = reader.GetDecimal("Amount"),
                .Currency = reader.GetString("Currency"),
                .Status = reader.GetString("Status"),
                .LiqPayStatus = If(reader.IsDBNull(reader.GetOrdinal("LiqPayStatus")), Nothing, reader.GetString("LiqPayStatus")),
                .PaymentId = If(reader.IsDBNull(reader.GetOrdinal("PaymentId")), Nothing, reader.GetString("PaymentId")),
                .DonorName = If(reader.IsDBNull(reader.GetOrdinal("DonorName")), Nothing, reader.GetString("DonorName")),
                .DonorEmail = If(reader.IsDBNull(reader.GetOrdinal("DonorEmail")), Nothing, reader.GetString("DonorEmail")),
                .CreatedAt = reader.GetDateTime("CreatedAt")
            }
        End Function

        ''' <summary>Створює запис Pending одразу перед перенаправленням на LiqPay. Повертає DonationId.</summary>
        Public Shared Function Create(orderId As String, amount As Decimal, donorName As String, donorEmail As String) As Integer
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "INSERT INTO Donations (OrderId, Amount, DonorName, DonorEmail, Status, CreatedAt) " &
                    "VALUES (@OrderId, @Amount, @DonorName, @DonorEmail, 'Pending', UTC_TIMESTAMP()); " &
                    "SELECT LAST_INSERT_ID();", conn)
                    cmd.Parameters.AddWithValue("@OrderId", orderId)
                    cmd.Parameters.AddWithValue("@Amount", amount)
                    cmd.Parameters.AddWithValue("@DonorName", If(String.IsNullOrWhiteSpace(donorName), DBNull.Value, CObj(donorName)))
                    cmd.Parameters.AddWithValue("@DonorEmail", If(String.IsNullOrWhiteSpace(donorEmail), DBNull.Value, CObj(donorEmail)))
                    Return Convert.ToInt32(cmd.ExecuteScalar())
                End Using
            End Using
        End Function

        Public Shared Function GetByOrderId(orderId As String) As Donation
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT DonationId, OrderId, Amount, Currency, Status, LiqPayStatus, PaymentId, DonorName, DonorEmail, CreatedAt " &
                    "FROM Donations WHERE OrderId = @OrderId;", conn)
                    cmd.Parameters.AddWithValue("@OrderId", orderId)
                    Using reader = cmd.ExecuteReader()
                        If Not reader.Read() Then Return Nothing
                        Return Map(reader)
                    End Using
                End Using
            End Using
        End Function

        ''' <summary>Оновлює статус за callback LiqPay (server_url). Ідемпотентно — повторний
        ''' callback з тим самим статусом просто перезаписує ті самі значення.</summary>
        Public Shared Function UpdateStatus(orderId As String, status As String, liqPayStatus As String, paymentId As String) As Boolean
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "UPDATE Donations SET Status = @Status, LiqPayStatus = @LiqPayStatus, PaymentId = @PaymentId, " &
                    "UpdatedAt = UTC_TIMESTAMP() WHERE OrderId = @OrderId;", conn)
                    cmd.Parameters.AddWithValue("@Status", status)
                    cmd.Parameters.AddWithValue("@LiqPayStatus", If(String.IsNullOrWhiteSpace(liqPayStatus), DBNull.Value, CObj(liqPayStatus)))
                    cmd.Parameters.AddWithValue("@PaymentId", If(String.IsNullOrWhiteSpace(paymentId), DBNull.Value, CObj(paymentId)))
                    cmd.Parameters.AddWithValue("@OrderId", orderId)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

    End Class

End Namespace
