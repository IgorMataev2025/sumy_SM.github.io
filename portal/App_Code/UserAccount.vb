Imports System
Imports System.Collections.Generic
Imports MySql.Data.MySqlClient

Namespace SumyPortal

    Public Enum AccountType
        Consumer
        Provider
    End Enum

    ''' <summary>
    ''' Обліковий запис користувача + доступ до таблиці Users (ТЗ, розділи 4.1, 7).
    ''' Усі запити — параметризовані (нефункціональна вимога, розділ 8).
    ''' </summary>
    Public Class UserAccount
        Public Property UserId As Integer
        Public Property Email As String
        Public Property FullName As String
        Public Property Phone As String
        Public Property UserType As String
        Public Property IsLegalEntity As Boolean
        Public Property CompanyName As String
        Public Property EDRPOU As String
        Public Property District As String
        Public Property IsActive As Boolean
        Public Property IsAdmin As Boolean
        Public Property EmailConfirmed As Boolean
        Public Property CreatedAt As DateTime

        ''' <summary>Дайджест на email (п.25, наступна фіча понад MVP, 2026-09-12) — коли
        ''' користувачу востаннє надіслано дайджест нових оголошень; Nothing — ще ніколи
        ''' (тоді відлік іде від CreatedAt, DigestSender.vb).</summary>
        Public Property LastDigestSentAt As DateTime?

        Private Const SelectColumns As String =
            "UserId, Email, FullName, Phone, UserType, IsLegalEntity, " &
            "CompanyName, EDRPOU, District, IsActive, IsAdmin, EmailConfirmed, CreatedAt, LastDigestSentAt "

        Private Shared Function Map(reader As MySqlDataReader) As UserAccount
            Return New UserAccount With {
                .UserId = reader.GetInt32("UserId"),
                .Email = reader.GetString("Email"),
                .FullName = reader.GetString("FullName"),
                .Phone = If(reader.IsDBNull(reader.GetOrdinal("Phone")), Nothing, reader.GetString("Phone")),
                .UserType = reader.GetString("UserType"),
                .IsLegalEntity = reader.GetBoolean("IsLegalEntity"),
                .CompanyName = If(reader.IsDBNull(reader.GetOrdinal("CompanyName")), Nothing, reader.GetString("CompanyName")),
                .EDRPOU = If(reader.IsDBNull(reader.GetOrdinal("EDRPOU")), Nothing, reader.GetString("EDRPOU")),
                .District = If(reader.IsDBNull(reader.GetOrdinal("District")), Nothing, reader.GetString("District")),
                .IsActive = reader.GetBoolean("IsActive"),
                .IsAdmin = reader.GetBoolean("IsAdmin"),
                .EmailConfirmed = reader.GetBoolean("EmailConfirmed"),
                .CreatedAt = reader.GetDateTime("CreatedAt"),
                .LastDigestSentAt = If(reader.IsDBNull(reader.GetOrdinal("LastDigestSentAt")), CType(Nothing, DateTime?), reader.GetDateTime("LastDigestSentAt"))
            }
        End Function

        ''' <summary>Поточний користувач за email (з Forms-автентифікації, Page.User.Identity.Name) — без пароля.</summary>
        Public Shared Function FindByEmail(email As String) As UserAccount
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand("SELECT " & SelectColumns & "FROM Users WHERE Email = @Email;", conn)
                    cmd.Parameters.AddWithValue("@Email", email)
                    Using reader = cmd.ExecuteReader()
                        If Not reader.Read() Then Return Nothing
                        Return Map(reader)
                    End Using
                End Using
            End Using
        End Function

        ''' <summary>Обліковий запис за Id — напр., дані постачальника для генерації договору (п.3 уточненої постановки).</summary>
        Public Shared Function GetById(userId As Integer) As UserAccount
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand("SELECT " & SelectColumns & "FROM Users WHERE UserId = @UserId;", conn)
                    cmd.Parameters.AddWithValue("@UserId", userId)
                    Using reader = cmd.ExecuteReader()
                        If Not reader.Read() Then Return Nothing
                        Return Map(reader)
                    End Using
                End Using
            End Using
        End Function

        ''' <summary>Усі користувачі — для адмін-панелі (ТЗ, розділ 4.3: "керування користувачами").</summary>
        Public Shared Function GetAll() As List(Of UserAccount)
            Dim result As New List(Of UserAccount)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand("SELECT " & SelectColumns & "FROM Users ORDER BY CreatedAt DESC;", conn)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            result.Add(Map(reader))
                        End While
                    End Using
                End Using
            End Using
            Return result
        End Function

        ''' <summary>Активні Споживачі з підтвердженим email, яким час надіслати дайджест
        ''' нових оголошень (п.25, наступна фіча понад MVP, 2026-09-12) — COALESCE бере
        ''' LastDigestSentAt, а для тих, хто ще не отримував жодного, CreatedAt (щоб не
        ''' слати дайджест одразу після реєстрації, а лише через days днів). Постачальники
        ''' дайджест не отримують — свідоме MVP-рішення (мета фічі — повернути споживачів).</summary>
        Public Shared Function GetDueForDigest(days As Integer) As List(Of UserAccount)
            Dim result As New List(Of UserAccount)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT " & SelectColumns & "FROM Users " &
                    "WHERE UserType = 'Consumer' AND IsActive = TRUE AND EmailConfirmed = TRUE " &
                    "AND COALESCE(LastDigestSentAt, CreatedAt) < DATE_SUB(UTC_TIMESTAMP(), INTERVAL @Days DAY);", conn)
                    cmd.Parameters.AddWithValue("@Days", days)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            result.Add(Map(reader))
                        End While
                    End Using
                End Using
            End Using
            Return result
        End Function

        ''' <summary>Фіксує момент надсилання дайджесту (п.25) — незалежно від того, чи
        ''' лист реально дійшов (той самий підхід, що вже архівація застарілих оголошень,
        ''' п.20: помилка листа не повинна ламати чи повторювати основну дію).</summary>
        Public Shared Sub MarkDigestSent(userId As Integer)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand("UPDATE Users SET LastDigestSentAt = UTC_TIMESTAMP() WHERE UserId = @UserId;", conn)
                    cmd.Parameters.AddWithValue("@UserId", userId)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        End Sub

        ''' <summary>Блокування/розблокування акаунта адміністратором (ТЗ, розділ 4.3). Адмінів блокувати не можна.</summary>
        Public Shared Function SetActive(userId As Integer, isActive As Boolean) As Boolean
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "UPDATE Users SET IsActive = @IsActive WHERE UserId = @UserId AND IsAdmin = FALSE;", conn)
                    cmd.Parameters.AddWithValue("@IsActive", isActive)
                    cmd.Parameters.AddWithValue("@UserId", userId)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        Public Shared Function EmailExists(email As String) As Boolean
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand("SELECT COUNT(*) FROM Users WHERE Email = @Email;", conn)
                    cmd.Parameters.AddWithValue("@Email", email)
                    Return Convert.ToInt32(cmd.ExecuteScalar()) > 0
                End Using
            End Using
        End Function

        ''' <summary>Реєструє нового користувача. Повертає токен підтвердження email.</summary>
        Public Shared Function Register(email As String, password As String, fullName As String, phone As String,
                                         accountType As AccountType, isLegalEntity As Boolean,
                                         companyName As String, edrpou As String, district As String) As String
            Dim passwordHash = PasswordHasher.Hash(password)
            Dim confirmationToken = PasswordHasher.GenerateToken()
            Dim userTypeStr = If(accountType = AccountType.Provider, "Provider", "Consumer")

            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "INSERT INTO Users (Email, PasswordHash, FullName, Phone, UserType, IsLegalEntity, " &
                    "CompanyName, EDRPOU, District, IsActive, EmailConfirmed, EmailConfirmationToken, CreatedAt) " &
                    "VALUES (@Email, @PasswordHash, @FullName, @Phone, @UserType, @IsLegalEntity, " &
                    "@CompanyName, @EDRPOU, @District, TRUE, FALSE, @Token, UTC_TIMESTAMP());", conn)
                    cmd.Parameters.AddWithValue("@Email", email)
                    cmd.Parameters.AddWithValue("@PasswordHash", passwordHash)
                    cmd.Parameters.AddWithValue("@FullName", fullName)
                    cmd.Parameters.AddWithValue("@Phone", If(String.IsNullOrWhiteSpace(phone), DBNull.Value, CObj(phone)))
                    cmd.Parameters.AddWithValue("@UserType", userTypeStr)
                    cmd.Parameters.AddWithValue("@IsLegalEntity", isLegalEntity)
                    cmd.Parameters.AddWithValue("@CompanyName", If(String.IsNullOrWhiteSpace(companyName), DBNull.Value, CObj(companyName)))
                    cmd.Parameters.AddWithValue("@EDRPOU", If(String.IsNullOrWhiteSpace(edrpou), DBNull.Value, CObj(edrpou)))
                    cmd.Parameters.AddWithValue("@District", If(String.IsNullOrWhiteSpace(district), DBNull.Value, CObj(district)))
                    cmd.Parameters.AddWithValue("@Token", confirmationToken)
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            Return confirmationToken
        End Function

        ''' <summary>
        ''' Перевіряє логін/пароль. errorMessage пояснює причину відмови українською
        ''' (невірні дані / заблоковано / email не підтверджено).
        ''' </summary>
        Public Shared Function ValidateLogin(email As String, password As String, ByRef errorMessage As String) As UserAccount
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT " & SelectColumns & ", PasswordHash FROM Users WHERE Email = @Email;", conn)
                    cmd.Parameters.AddWithValue("@Email", email)
                    Using reader = cmd.ExecuteReader()
                        If Not reader.Read() Then
                            errorMessage = "Невірний email або пароль."
                            Return Nothing
                        End If

                        Dim storedHash = reader.GetString("PasswordHash")
                        If Not PasswordHasher.Verify(password, storedHash) Then
                            errorMessage = "Невірний email або пароль."
                            Return Nothing
                        End If

                        Dim account = Map(reader)

                        If Not account.IsActive Then
                            errorMessage = "Обліковий запис заблоковано адміністратором."
                            Return Nothing
                        End If
                        If Not account.EmailConfirmed Then
                            errorMessage = "Email ще не підтверджено. Перевірте посилання, надіслане при реєстрації."
                            Return Nothing
                        End If

                        errorMessage = Nothing
                        Return account
                    End Using
                End Using
            End Using
        End Function

        Public Shared Function ConfirmEmail(token As String) As Boolean
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "UPDATE Users SET EmailConfirmed = TRUE, EmailConfirmationToken = NULL " &
                    "WHERE EmailConfirmationToken = @Token;", conn)
                    cmd.Parameters.AddWithValue("@Token", token)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        ''' <summary>
        ''' Генерує токен скидання пароля (дійсний 1 годину), якщо такий email існує.
        ''' Повертає Nothing, якщо email не знайдено — виклику НЕ варто розкривати
        ''' цей факт користувачу (щоб не давати перевіряти, які email зареєстровані).
        ''' </summary>
        Public Shared Function CreatePasswordResetToken(email As String) As String
            Dim token = PasswordHasher.GenerateToken()

            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "UPDATE Users SET PasswordResetToken = @Token, " &
                    "PasswordResetExpires = DATE_ADD(UTC_TIMESTAMP(), INTERVAL 1 HOUR) " &
                    "WHERE Email = @Email;", conn)
                    cmd.Parameters.AddWithValue("@Token", token)
                    cmd.Parameters.AddWithValue("@Email", email)
                    Dim affected = cmd.ExecuteNonQuery()
                    If affected = 0 Then Return Nothing
                End Using
            End Using

            Return token
        End Function

        ''' <summary>Редагування власних даних із особистого кабінету (Profile.aspx, п.6 уточненої постановки). Email і тип акаунта тут не змінюються.</summary>
        Public Shared Function UpdateProfile(userId As Integer, fullName As String, phone As String, district As String,
                                              companyName As String, edrpou As String) As Boolean
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "UPDATE Users SET FullName = @FullName, Phone = @Phone, District = @District, " &
                    "CompanyName = @CompanyName, EDRPOU = @EDRPOU WHERE UserId = @UserId;", conn)
                    cmd.Parameters.AddWithValue("@FullName", fullName)
                    cmd.Parameters.AddWithValue("@Phone", If(String.IsNullOrWhiteSpace(phone), DBNull.Value, CObj(phone)))
                    cmd.Parameters.AddWithValue("@District", If(String.IsNullOrWhiteSpace(district), DBNull.Value, CObj(district)))
                    cmd.Parameters.AddWithValue("@CompanyName", If(String.IsNullOrWhiteSpace(companyName), DBNull.Value, CObj(companyName)))
                    cmd.Parameters.AddWithValue("@EDRPOU", If(String.IsNullOrWhiteSpace(edrpou), DBNull.Value, CObj(edrpou)))
                    cmd.Parameters.AddWithValue("@UserId", userId)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        ''' <summary>
        ''' Самостійне видалення акаунта (Profile.aspx, п.6 — реалізує право суб'єкта
        ''' персональних даних на видалення з PrivacyPolicy.aspx без звернення на
        ''' email адміністратора). Знеособлення, а не фізичне видалення рядка —
        ''' Users.UserId лишається як FK у Services/ModerationLog/AdminId (referential
        ''' integrity), тому email/ПІБ/телефон/реквізити затираються, а вхід
        ''' блокується через IsActive (той самий механізм, що адмінська блокировка
        ''' в AdminUsers.aspx). Адмінів самовидалення не стосується.
        ''' </summary>
        Public Shared Function DeleteAccount(userId As Integer) As Boolean
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "UPDATE Users SET FullName = 'Видалений користувач', Phone = NULL, " &
                    "CompanyName = NULL, EDRPOU = NULL, District = NULL, " &
                    "Email = CONCAT('deleted_', @UserId, '@safina.invalid'), " &
                    "PasswordHash = @RandomHash, IsActive = FALSE " &
                    "WHERE UserId = @UserId AND IsAdmin = FALSE;", conn)
                    cmd.Parameters.AddWithValue("@RandomHash", PasswordHasher.Hash(Guid.NewGuid().ToString("N")))
                    cmd.Parameters.AddWithValue("@UserId", userId)
                    If cmd.ExecuteNonQuery() = 0 Then Return False
                End Using

                ' Опубліковані/на модерації оголошення теж несуть персональні дані
                ' (контактний телефон) — знімаємо з публікації тим самим прийомом,
                ' що Service.Unpublish.
                Using cmd2 As New MySqlCommand(
                    "UPDATE Services SET Status = 'Draft' WHERE ProviderId = @UserId AND Status IN ('Approved', 'Pending');", conn)
                    cmd2.Parameters.AddWithValue("@UserId", userId)
                    cmd2.ExecuteNonQuery()
                End Using
            End Using
            Return True
        End Function

        Public Shared Function ResetPassword(token As String, newPassword As String) As Boolean
            Using conn = DbHelper.GetConnection()
                Using checkCmd As New MySqlCommand(
                    "SELECT UserId FROM Users WHERE PasswordResetToken = @Token " &
                    "AND PasswordResetExpires > UTC_TIMESTAMP();", conn)
                    checkCmd.Parameters.AddWithValue("@Token", token)
                    Dim userIdObj = checkCmd.ExecuteScalar()
                    If userIdObj Is Nothing Then Return False
                End Using

                Using updateCmd As New MySqlCommand(
                    "UPDATE Users SET PasswordHash = @Hash, PasswordResetToken = NULL, " &
                    "PasswordResetExpires = NULL WHERE PasswordResetToken = @Token;", conn)
                    updateCmd.Parameters.AddWithValue("@Hash", PasswordHasher.Hash(newPassword))
                    updateCmd.Parameters.AddWithValue("@Token", token)
                    Return updateCmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

    End Class

End Namespace
