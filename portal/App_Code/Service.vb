Imports System
Imports System.Collections.Generic
Imports System.Text
Imports MySql.Data.MySqlClient

Namespace SumyPortal

    ''' <summary>
    ''' Оголошення послуги + доступ до таблиці Services (ТЗ, розділи 4.2, 7).
    ''' Статуси: Чернетка(Draft) → На модерації(Pending) → Опубліковано(Approved) / Відхилено(Rejected).
    ''' Усі запити — параметризовані (нефункціональна вимога, розділ 8).
    ''' </summary>
    Public Class Service
        Public Property ServiceId As Integer
        Public Property ProviderId As Integer
        Public Property CategoryId As Integer
        Public Property CategoryName As String
        Public Property Title As String
        Public Property Description As String
        Public Property Price As Decimal?
        Public Property District As String
        Public Property Phone As String

        ''' <summary>Геолокація оголошення (постановка робочої тестової версії, 2026-09-12) —
        ''' клік на карті в ServiceEdit.aspx, лише інформативна мітка на ServiceDetails.aspx.
        ''' Доповнює District (район), не замінює його. Nothing — постачальник ще не ставив мітку.</summary>
        Public Property Latitude As Decimal?
        Public Property Longitude As Decimal?

        Public Property Status As String
        Public Property RejectReason As String
        Public Property CreatedAt As DateTime

        ''' <summary>Лічильник переглядів ServiceDetails.aspx (п.17, статистика для постачальника,
        ''' 2026-09-12) — +1 при кожному не-постбек завантаженні опублікованого оголошення,
        ''' включно з переглядами самого власника (свідомо не фільтруємо).</summary>
        Public Property ViewCount As Integer

        ''' <summary>Позначка "Перевірено адміном" (п.22, наступна фіча понад MVP, 2026-09-12) —
        ''' НЕ заміняє звичайну модерацію (Status), а додатковий сигнал довіри, який адмін
        ''' виставляє на власний розсуд (AdminServiceEdit.aspx). Публічно видима (Catalog.aspx/
        ''' ServiceDetails.aspx) лише разом зі Status = Approved.</summary>
        Public Property IsVerified As Boolean

        ''' <summary>Заповнюється лише для черги модерації і картки оголошення — контакти постачальника.</summary>
        Public Property ProviderName As String
        Public Property ProviderEmail As String

        ''' <summary>Заповнюється лише для каталогу (Catalog.aspx) — шлях до першого фото, якщо є.</summary>
        Public Property ThumbnailUrl As String

        ''' <summary>Заповнюються лише для MyServices.aspx (п.17, статистика для постачальника) —
        ''' розмови/вподобання/відгуки рахуються "на льоту" з наявних таблиць окремим запитом
        ''' (BindStats у MyServices.aspx.vb), а не тут — Map(reader) їх не знає.</summary>
        Public Property ConversationCount As Integer
        Public Property FavoriteCount As Integer
        Public Property ReviewAverage As Decimal?
        Public Property ReviewCount As Integer

        Public ReadOnly Property StatusLabel As String
            Get
                Select Case Status
                    Case "Draft" : Return "Чернетка"
                    Case "Pending" : Return "На модерації"
                    Case "Approved" : Return "Опубліковано"
                    Case "Rejected" : Return "Відхилено"
                    Case Else : Return Status
                End Select
            End Get
        End Property

        ''' <summary>Friend (не Private) — потрібна й з Favorite.vb (та сама збірка App_Code) для GetByUser.</summary>
        Friend Shared Function Map(reader As MySqlDataReader) As Service
            Return New Service With {
                .ServiceId = reader.GetInt32("ServiceId"),
                .ProviderId = reader.GetInt32("ProviderId"),
                .CategoryId = reader.GetInt32("CategoryId"),
                .CategoryName = reader.GetString("CategoryName"),
                .Title = reader.GetString("Title"),
                .Description = If(reader.IsDBNull(reader.GetOrdinal("Description")), Nothing, reader.GetString("Description")),
                .Price = If(reader.IsDBNull(reader.GetOrdinal("Price")), CType(Nothing, Decimal?), reader.GetDecimal("Price")),
                .District = If(reader.IsDBNull(reader.GetOrdinal("District")), Nothing, reader.GetString("District")),
                .Phone = If(reader.IsDBNull(reader.GetOrdinal("Phone")), Nothing, reader.GetString("Phone")),
                .Latitude = If(reader.IsDBNull(reader.GetOrdinal("Latitude")), CType(Nothing, Decimal?), reader.GetDecimal("Latitude")),
                .Longitude = If(reader.IsDBNull(reader.GetOrdinal("Longitude")), CType(Nothing, Decimal?), reader.GetDecimal("Longitude")),
                .Status = reader.GetString("Status"),
                .RejectReason = If(reader.IsDBNull(reader.GetOrdinal("RejectReason")), Nothing, reader.GetString("RejectReason")),
                .CreatedAt = reader.GetDateTime("CreatedAt"),
                .ViewCount = reader.GetInt32("ViewCount"),
                .IsVerified = reader.GetBoolean("IsVerified")
            }
        End Function

        ''' <summary>
        ''' Map() читає Latitude/Longitude/ViewCount/IsVerified безумовно (reader.GetOrdinal/
        ''' GetInt32/GetBoolean) — тому будь-який SELECT, переданий у Map(reader), ОБОВ'ЯЗКОВО
        ''' має містити ці стовпці, інакше IndexOutOfRangeException. Стосується й інших
        ''' inline-запитів нижче (GetPendingForModeration/GetPendingById/GetApprovedById/
        ''' GetForMessaging/GetAllForAdmin/GetByIdAny/SearchApproved/SearchApprovedForMap), що
        ''' дублюють цей самий список колонок — так само Favorite.vb: GetByUser (та сама збірка
        ''' App_Code). GetApprovedForSitemap — виняток: свідомо вузький SELECT лише ServiceId/
        ''' ApprovedAt, Map(reader) там не використовується.
        ''' </summary>
        Private Const SelectBase As String =
            "SELECT s.ServiceId, s.ProviderId, s.CategoryId, c.Name AS CategoryName, s.Title, s.Description, " &
            "s.Price, s.District, s.Phone, s.Latitude, s.Longitude, s.Status, s.RejectReason, s.CreatedAt, " &
            "s.ViewCount, s.IsVerified " &
            "FROM Services s JOIN Categories c ON c.CategoryId = s.CategoryId "

        ''' <summary>Усі оголошення постачальника, найновіші зверху.</summary>
        Public Shared Function GetByProvider(providerId As Integer) As List(Of Service)
            Dim result As New List(Of Service)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(SelectBase & "WHERE s.ProviderId = @ProviderId ORDER BY s.CreatedAt DESC;", conn)
                    cmd.Parameters.AddWithValue("@ProviderId", providerId)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            result.Add(Map(reader))
                        End While
                    End Using
                End Using
            End Using
            Return result
        End Function

        ''' <summary>Оголошення за Id — лише якщо належить цьому постачальнику, інакше Nothing.</summary>
        Public Shared Function GetById(serviceId As Integer, providerId As Integer) As Service
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(SelectBase & "WHERE s.ServiceId = @ServiceId AND s.ProviderId = @ProviderId;", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    cmd.Parameters.AddWithValue("@ProviderId", providerId)
                    Using reader = cmd.ExecuteReader()
                        If reader.Read() Then Return Map(reader)
                        Return Nothing
                    End Using
                End Using
            End Using
        End Function

        ''' <summary>Створює нове оголошення у статусі "Чернетка". Повертає ServiceId.</summary>
        Public Shared Function Create(providerId As Integer, categoryId As Integer, title As String, description As String,
                                       price As Decimal?, district As String, phone As String,
                                       latitude As Decimal?, longitude As Decimal?) As Integer
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "INSERT INTO Services (ProviderId, CategoryId, Title, Description, Price, District, Phone, Latitude, Longitude, Status, CreatedAt) " &
                    "VALUES (@ProviderId, @CategoryId, @Title, @Description, @Price, @District, @Phone, @Latitude, @Longitude, 'Draft', UTC_TIMESTAMP()); " &
                    "SELECT LAST_INSERT_ID();", conn)
                    AddCommonParams(cmd, providerId, categoryId, title, description, price, district, phone, latitude, longitude)
                    Return Convert.ToInt32(cmd.ExecuteScalar())
                End Using
            End Using
        End Function

        ''' <summary>Редагування власного оголошення (тільки поки Draft або Rejected — після подачі на модерацію
        ''' зміни заборонені до вирішення модератора). Повертає False, якщо не знайдено/не власник/не той статус.</summary>
        Public Shared Function Update(serviceId As Integer, providerId As Integer, categoryId As Integer, title As String,
                                       description As String, price As Decimal?, district As String, phone As String,
                                       latitude As Decimal?, longitude As Decimal?) As Boolean
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "UPDATE Services SET CategoryId = @CategoryId, Title = @Title, Description = @Description, " &
                    "Price = @Price, District = @District, Phone = @Phone, Latitude = @Latitude, Longitude = @Longitude " &
                    "WHERE ServiceId = @ServiceId AND ProviderId = @ProviderId AND Status IN ('Draft', 'Rejected');", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    AddCommonParams(cmd, providerId, categoryId, title, description, price, district, phone, latitude, longitude)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        Private Shared Sub AddCommonParams(cmd As MySqlCommand, providerId As Integer, categoryId As Integer, title As String,
                                            description As String, price As Decimal?, district As String, phone As String,
                                            latitude As Decimal?, longitude As Decimal?)
            cmd.Parameters.AddWithValue("@ProviderId", providerId)
            cmd.Parameters.AddWithValue("@CategoryId", categoryId)
            cmd.Parameters.AddWithValue("@Title", title)
            cmd.Parameters.AddWithValue("@Description", If(String.IsNullOrWhiteSpace(description), DBNull.Value, CObj(description)))
            cmd.Parameters.AddWithValue("@Price", If(price.HasValue, CObj(price.Value), DBNull.Value))
            cmd.Parameters.AddWithValue("@District", If(String.IsNullOrWhiteSpace(district), DBNull.Value, CObj(district)))
            cmd.Parameters.AddWithValue("@Phone", If(String.IsNullOrWhiteSpace(phone), DBNull.Value, CObj(phone)))
            cmd.Parameters.AddWithValue("@Latitude", If(latitude.HasValue, CObj(latitude.Value), DBNull.Value))
            cmd.Parameters.AddWithValue("@Longitude", If(longitude.HasValue, CObj(longitude.Value), DBNull.Value))
        End Sub

        ''' <summary>Подати на модерацію: Draft/Rejected → Pending.</summary>
        Public Shared Function SubmitForModeration(serviceId As Integer, providerId As Integer) As Boolean
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "UPDATE Services SET Status = 'Pending', RejectReason = NULL " &
                    "WHERE ServiceId = @ServiceId AND ProviderId = @ProviderId AND Status IN ('Draft', 'Rejected');", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    cmd.Parameters.AddWithValue("@ProviderId", providerId)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        ''' <summary>Зняти з публікації: Approved → Draft (ТЗ, розділ 4.2 — "зняття з публікації").</summary>
        Public Shared Function Unpublish(serviceId As Integer, providerId As Integer) As Boolean
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "UPDATE Services SET Status = 'Draft' " &
                    "WHERE ServiceId = @ServiceId AND ProviderId = @ProviderId AND Status = 'Approved';", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    cmd.Parameters.AddWithValue("@ProviderId", providerId)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        ' --- Фото ---

        Public Shared Function GetPhotos(serviceId As Integer) As List(Of ServicePhoto)
            Dim result As New List(Of ServicePhoto)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand("SELECT PhotoId, ServiceId, FilePath FROM ServicePhotos WHERE ServiceId = @ServiceId ORDER BY PhotoId;", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            result.Add(New ServicePhoto With {
                                .PhotoId = reader.GetInt32("PhotoId"),
                                .ServiceId = reader.GetInt32("ServiceId"),
                                .FilePath = reader.GetString("FilePath")
                            })
                        End While
                    End Using
                End Using
            End Using
            Return result
        End Function

        Public Shared Function CountPhotos(serviceId As Integer) As Integer
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand("SELECT COUNT(*) FROM ServicePhotos WHERE ServiceId = @ServiceId;", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    Return Convert.ToInt32(cmd.ExecuteScalar())
                End Using
            End Using
        End Function

        Public Shared Sub AddPhoto(serviceId As Integer, filePath As String)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand("INSERT INTO ServicePhotos (ServiceId, FilePath) VALUES (@ServiceId, @FilePath);", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    cmd.Parameters.AddWithValue("@FilePath", filePath)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        End Sub

        ''' <summary>Видаляє запис фото, якщо воно належить оголошенню цього постачальника. Повертає шлях файлу для видалення з диска (або Nothing).</summary>
        Public Shared Function DeletePhoto(photoId As Integer, providerId As Integer) As String
            Using conn = DbHelper.GetConnection()
                Dim filePath As String = Nothing
                Using selectCmd As New MySqlCommand(
                    "SELECT p.FilePath FROM ServicePhotos p JOIN Services s ON s.ServiceId = p.ServiceId " &
                    "WHERE p.PhotoId = @PhotoId AND s.ProviderId = @ProviderId;", conn)
                    selectCmd.Parameters.AddWithValue("@PhotoId", photoId)
                    selectCmd.Parameters.AddWithValue("@ProviderId", providerId)
                    Dim result = selectCmd.ExecuteScalar()
                    If result Is Nothing Then Return Nothing
                    filePath = CStr(result)
                End Using

                Using deleteCmd As New MySqlCommand("DELETE FROM ServicePhotos WHERE PhotoId = @PhotoId;", conn)
                    deleteCmd.Parameters.AddWithValue("@PhotoId", photoId)
                    deleteCmd.ExecuteNonQuery()
                End Using

                Return filePath
            End Using
        End Function

        ' --- Модерація (адміністратор, ТЗ п.4.3) ---

        ''' <summary>Черга "на модерації", найстаріші спершу (FIFO) — з даними постачальника для контексту адміна.</summary>
        Public Shared Function GetPendingForModeration() As List(Of Service)
            Dim result As New List(Of Service)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT s.ServiceId, s.ProviderId, s.CategoryId, c.Name AS CategoryName, s.Title, s.Description, " &
                    "s.Price, s.District, s.Phone, s.Latitude, s.Longitude, s.Status, s.RejectReason, s.CreatedAt, s.ViewCount, s.IsVerified, " &
                    "u.FullName AS ProviderName, u.Email AS ProviderEmail " &
                    "FROM Services s " &
                    "JOIN Categories c ON c.CategoryId = s.CategoryId " &
                    "JOIN Users u ON u.UserId = s.ProviderId " &
                    "WHERE s.Status = 'Pending' ORDER BY s.CreatedAt ASC;", conn)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim svc = Map(reader)
                            svc.ProviderName = reader.GetString("ProviderName")
                            svc.ProviderEmail = reader.GetString("ProviderEmail")
                            result.Add(svc)
                        End While
                    End Using
                End Using
            End Using
            Return result
        End Function

        ''' <summary>Одне оголошення "на модерації" за Id, з контактами постачальника —
        ''' для email-сповіщення (AdminModeration.aspx), знімається ДО зміни статусу.</summary>
        Public Shared Function GetPendingById(serviceId As Integer) As Service
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT s.ServiceId, s.ProviderId, s.CategoryId, c.Name AS CategoryName, s.Title, s.Description, " &
                    "s.Price, s.District, s.Phone, s.Latitude, s.Longitude, s.Status, s.RejectReason, s.CreatedAt, s.ViewCount, s.IsVerified, " &
                    "u.FullName AS ProviderName, u.Email AS ProviderEmail " &
                    "FROM Services s " &
                    "JOIN Categories c ON c.CategoryId = s.CategoryId " &
                    "JOIN Users u ON u.UserId = s.ProviderId " &
                    "WHERE s.ServiceId = @ServiceId AND s.Status = 'Pending';", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    Using reader = cmd.ExecuteReader()
                        If Not reader.Read() Then Return Nothing
                        Dim svc = Map(reader)
                        svc.ProviderName = reader.GetString("ProviderName")
                        svc.ProviderEmail = reader.GetString("ProviderEmail")
                        Return svc
                    End Using
                End Using
            End Using
        End Function

        ''' <summary>Схвалити: Pending → Approved. Пише запис у ModerationLog.</summary>
        Public Shared Function Approve(serviceId As Integer, adminId As Integer) As Boolean
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "UPDATE Services SET Status = 'Approved', RejectReason = NULL, " &
                    "ApprovedAt = UTC_TIMESTAMP(), ApprovedBy = @AdminId " &
                    "WHERE ServiceId = @ServiceId AND Status = 'Pending';", conn)
                    cmd.Parameters.AddWithValue("@AdminId", adminId)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    If cmd.ExecuteNonQuery() = 0 Then Return False
                End Using

                LogModeration(conn, serviceId, adminId, "Approved", Nothing)
                Return True
            End Using
        End Function

        ''' <summary>Відхилити: Pending → Rejected, з причиною, видимою постачальнику. Пише запис у ModerationLog.</summary>
        Public Shared Function Reject(serviceId As Integer, adminId As Integer, reason As String) As Boolean
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "UPDATE Services SET Status = 'Rejected', RejectReason = @Reason " &
                    "WHERE ServiceId = @ServiceId AND Status = 'Pending';", conn)
                    cmd.Parameters.AddWithValue("@Reason", reason)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    If cmd.ExecuteNonQuery() = 0 Then Return False
                End Using

                LogModeration(conn, serviceId, adminId, "Rejected", reason)
                Return True
            End Using
        End Function

        Private Shared Sub LogModeration(conn As MySqlConnection, serviceId As Integer, adminId As Integer, action As String, comment As String)
            Using cmd As New MySqlCommand(
                "INSERT INTO ModerationLog (ServiceId, AdminId, Action, Comment, ActionDate) " &
                "VALUES (@ServiceId, @AdminId, @Action, @Comment, UTC_TIMESTAMP());", conn)
                cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                cmd.Parameters.AddWithValue("@AdminId", adminId)
                cmd.Parameters.AddWithValue("@Action", action)
                cmd.Parameters.AddWithValue("@Comment", If(String.IsNullOrWhiteSpace(comment), DBNull.Value, CObj(comment)))
                cmd.ExecuteNonQuery()
            End Using
        End Sub

        ' --- Статистика для постачальника (п.17, наступна фіча понад MVP, 2026-09-12) ---

        ''' <summary>+1 до лічильника переглядів. Викликається з ServiceDetails.aspx.vb лише при
        ''' не-постбек завантаженні (кожен F5/перехід — новий перегляд); власні перегляди
        ''' постачальника теж рахуються (рішення користувача — не фільтруємо).</summary>
        Public Shared Sub IncrementViewCount(serviceId As Integer)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand("UPDATE Services SET ViewCount = ViewCount + 1 WHERE ServiceId = @ServiceId;", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        End Sub

        ''' <summary>Кількість унікальних споживачів, що написали по цьому оголошенню (MyServices.aspx,
        ''' статистика) — рахується "на льоту" з Messages, окремого лічильника в БД не заведено.</summary>
        Public Shared Function GetConversationCount(serviceId As Integer) As Integer
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT COUNT(DISTINCT ConsumerId) FROM Messages WHERE ServiceId = @ServiceId;", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    Return Convert.ToInt32(cmd.ExecuteScalar())
                End Using
            End Using
        End Function

        ''' <summary>Автоматичне зняття застарілих оголошень (п.20, наступна фіча понад MVP,
        ''' 2026-09-12) — опубліковане оголошення, що не оновлювалось понад заданий поріг
        ''' днів від дати публікації (ApprovedAt), знімається з публікації (Approved → Draft,
        ''' та сама дія, що ручна кнопка "Зняти з публікації" в MyServices.aspx). Причина
        ''' пишеться в RejectReason — той самий стовпець і той самий блок на MyServices.aspx,
        ''' що вже показує причину відхилення адміном (без нового поля/UI). Чернетки й
        ''' оголошення на модерації не чіпає — лише вже опубліковані. Повертає список знятих
        ''' оголошень (із контактами постачальника) — для email-сповіщення (Global.asax.vb).</summary>
        Public Shared Function ArchiveStaleApproved(days As Integer) As List(Of Service)
            Dim result As New List(Of Service)
            Dim reason = String.Format("Автоматично знято з публікації — оголошення не оновлювалося понад {0} днів.", days)

            Using conn = DbHelper.GetConnection()
                Using selectCmd As New MySqlCommand(
                    "SELECT s.ServiceId, s.Title, u.FullName AS ProviderName, u.Email AS ProviderEmail " &
                    "FROM Services s JOIN Users u ON u.UserId = s.ProviderId " &
                    "WHERE s.Status = 'Approved' AND s.ApprovedAt < DATE_SUB(UTC_TIMESTAMP(), INTERVAL @Days DAY);", conn)
                    selectCmd.Parameters.AddWithValue("@Days", days)
                    Using reader = selectCmd.ExecuteReader()
                        While reader.Read()
                            result.Add(New Service With {
                                .ServiceId = reader.GetInt32("ServiceId"),
                                .Title = reader.GetString("Title"),
                                .ProviderName = reader.GetString("ProviderName"),
                                .ProviderEmail = reader.GetString("ProviderEmail")
                            })
                        End While
                    End Using
                End Using

                ' Окремий UPDATE на кожен рядок (а не один пакетний WHERE IN) — той самий
                ' стиль, що вже в проєкті (напр. ProcessPhotoDeletions), і дає точний
                ' захист "лише якщо ще Approved" на випадок, якщо стан устиг змінитись
                ' між SELECT і UPDATE (напр. постачальник щойно сам зняв публікацію).
                For Each svc In result
                    Using updateCmd As New MySqlCommand(
                        "UPDATE Services SET Status = 'Draft', RejectReason = @Reason " &
                        "WHERE ServiceId = @ServiceId AND Status = 'Approved';", conn)
                        updateCmd.Parameters.AddWithValue("@Reason", reason)
                        updateCmd.Parameters.AddWithValue("@ServiceId", svc.ServiceId)
                        updateCmd.ExecuteNonQuery()
                    End Using
                Next
            End Using

            Return result
        End Function

        ''' <summary>Мінімальний список опублікованих оголошень для Sitemap.ashx (п.21,
        ''' наступна фіча понад MVP — базове SEO, 2026-09-12) — лише ServiceId/ApprovedAt,
        ''' навмисно свій вузький SELECT замість Map(reader): нічого іншого тут не треба,
        ''' і це уникає звички дописувати нове поле в усі 9+ місць, де Map(reader)
        ''' використовується (той самий "грабельний" список колонок, що вже для
        ''' Latitude/Longitude/ViewCount, п.13/п.17) — коли потрібні лише 1-2 стовпці,
        ''' простіше й безпечніше написати вузький запит, ніж тягнути повний SelectBase.</summary>
        Public Shared Function GetApprovedForSitemap() As List(Of KeyValuePair(Of Integer, DateTime))
            Dim result As New List(Of KeyValuePair(Of Integer, DateTime))
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand("SELECT ServiceId, ApprovedAt FROM Services WHERE Status = 'Approved';", conn)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            result.Add(New KeyValuePair(Of Integer, DateTime)(reader.GetInt32("ServiceId"), reader.GetDateTime("ApprovedAt")))
                        End While
                    End Using
                End Using
            End Using
            Return result
        End Function

        ' --- Каталог і пошук (споживач, ТЗ п.4.4) ---

        ''' <summary>Опубліковане оголошення за Id — для картки. Nothing, якщо не існує або не Approved.</summary>
        Public Shared Function GetApprovedById(serviceId As Integer) As Service
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT s.ServiceId, s.ProviderId, s.CategoryId, c.Name AS CategoryName, s.Title, s.Description, " &
                    "s.Price, s.District, s.Phone, s.Latitude, s.Longitude, s.Status, s.RejectReason, s.CreatedAt, s.ViewCount, s.IsVerified, " &
                    "u.FullName AS ProviderName, u.Email AS ProviderEmail " &
                    "FROM Services s " &
                    "JOIN Categories c ON c.CategoryId = s.CategoryId " &
                    "JOIN Users u ON u.UserId = s.ProviderId " &
                    "WHERE s.ServiceId = @ServiceId AND s.Status = 'Approved';", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    Using reader = cmd.ExecuteReader()
                        If Not reader.Read() Then Return Nothing
                        Dim svc = Map(reader)
                        svc.ProviderName = reader.GetString("ProviderName")
                        svc.ProviderEmail = reader.GetString("ProviderEmail")
                        Return svc
                    End Using
                End Using
            End Using
        End Function

        ''' <summary>Оголошення за Id незалежно від статусу — для сторінок повідомлень (п.10, обмін
        ''' Постачальник↔Споживач): розмова лишається доступною, навіть якщо оголошення пізніше
        ''' зняли з публікації. Nothing, якщо оголошення взагалі не існує.</summary>
        Public Shared Function GetForMessaging(serviceId As Integer) As Service
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT s.ServiceId, s.ProviderId, s.CategoryId, c.Name AS CategoryName, s.Title, s.Description, " &
                    "s.Price, s.District, s.Phone, s.Latitude, s.Longitude, s.Status, s.RejectReason, s.CreatedAt, s.ViewCount, s.IsVerified, " &
                    "u.FullName AS ProviderName, u.Email AS ProviderEmail " &
                    "FROM Services s " &
                    "JOIN Categories c ON c.CategoryId = s.CategoryId " &
                    "JOIN Users u ON u.UserId = s.ProviderId " &
                    "WHERE s.ServiceId = @ServiceId;", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    Using reader = cmd.ExecuteReader()
                        If Not reader.Read() Then Return Nothing
                        Dim svc = Map(reader)
                        svc.ProviderName = reader.GetString("ProviderName")
                        svc.ProviderEmail = reader.GetString("ProviderEmail")
                        Return svc
                    End Using
                End Using
            End Using
        End Function

        Private Shared Function BuildSearchWhere(categoryId As Integer?, district As String, keyword As String,
                                                  minPrice As Decimal?, maxPrice As Decimal?,
                                                  parameters As List(Of MySqlParameter)) As String
            Dim sb As New StringBuilder("WHERE s.Status = 'Approved' ")

            If categoryId.HasValue Then
                sb.Append("AND s.CategoryId = @CategoryId ")
                parameters.Add(New MySqlParameter("@CategoryId", categoryId.Value))
            End If
            If Not String.IsNullOrWhiteSpace(district) Then
                sb.Append("AND s.District = @District ")
                parameters.Add(New MySqlParameter("@District", district))
            End If
            If Not String.IsNullOrWhiteSpace(keyword) Then
                sb.Append("AND (s.Title LIKE @Keyword OR s.Description LIKE @Keyword) ")
                parameters.Add(New MySqlParameter("@Keyword", "%" & keyword & "%"))
            End If
            If minPrice.HasValue Then
                sb.Append("AND s.Price >= @MinPrice ")
                parameters.Add(New MySqlParameter("@MinPrice", minPrice.Value))
            End If
            If maxPrice.HasValue Then
                sb.Append("AND s.Price <= @MaxPrice ")
                parameters.Add(New MySqlParameter("@MaxPrice", maxPrice.Value))
            End If

            Return sb.ToString()
        End Function

        ''' <summary>Пошук опублікованих оголошень з фільтрами і пагінацією (ТЗ, розділ 4.4).</summary>
        Public Shared Function SearchApproved(categoryId As Integer?, district As String, keyword As String,
                                               minPrice As Decimal?, maxPrice As Decimal?,
                                               pageNumber As Integer, pageSize As Integer,
                                               ByRef totalCount As Integer) As List(Of Service)
            Dim result As New List(Of Service)

            Using conn = DbHelper.GetConnection()
                Dim countParams As New List(Of MySqlParameter)
                Dim whereSql = BuildSearchWhere(categoryId, district, keyword, minPrice, maxPrice, countParams)

                Using countCmd As New MySqlCommand("SELECT COUNT(*) FROM Services s " & whereSql & ";", conn)
                    countCmd.Parameters.AddRange(countParams.ToArray())
                    totalCount = Convert.ToInt32(countCmd.ExecuteScalar())
                End Using

                ' MySqlParameter не можна повторно прив'язати до іншої команди — будуємо WHERE ще раз зі свіжими параметрами.
                Dim selectParams As New List(Of MySqlParameter)
                Dim whereSql2 = BuildSearchWhere(categoryId, district, keyword, minPrice, maxPrice, selectParams)

                Using cmd As New MySqlCommand(
                    "SELECT s.ServiceId, s.ProviderId, s.CategoryId, c.Name AS CategoryName, s.Title, s.Description, " &
                    "s.Price, s.District, s.Phone, s.Latitude, s.Longitude, s.Status, s.RejectReason, s.CreatedAt, s.ViewCount, s.IsVerified " &
                    "FROM Services s JOIN Categories c ON c.CategoryId = s.CategoryId " & whereSql2 &
                    "ORDER BY s.CreatedAt DESC LIMIT @PageSize OFFSET @Offset;", conn)
                    cmd.Parameters.AddRange(selectParams.ToArray())
                    cmd.Parameters.AddWithValue("@PageSize", pageSize)
                    cmd.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            result.Add(Map(reader))
                        End While
                    End Using
                End Using
            End Using

            Return result
        End Function

        ''' <summary>Опубліковані оголошення з міткою на карті, що відповідають тим самим фільтрам,
        ''' що й SearchApproved (категорія/район/ключове слово/ціна) — для перемикача "Карта" на
        ''' Catalog.aspx (продовження геолокації, п.13). Без пагінації — карта показує всі відповідні
        ''' одразу, це інший спосіб огляду, не по PageSize. Оголошення без мітки (Latitude/Longitude
        ''' Nothing — постачальник її не ставив) сюди не потрапляють, лишаючись доступними у звичайному
        ''' списку.</summary>
        Public Shared Function SearchApprovedForMap(categoryId As Integer?, district As String, keyword As String,
                                                      minPrice As Decimal?, maxPrice As Decimal?) As List(Of Service)
            Dim result As New List(Of Service)

            Using conn = DbHelper.GetConnection()
                Dim parameters As New List(Of MySqlParameter)
                Dim whereSql = BuildSearchWhere(categoryId, district, keyword, minPrice, maxPrice, parameters)
                whereSql &= "AND s.Latitude IS NOT NULL AND s.Longitude IS NOT NULL "

                Using cmd As New MySqlCommand(
                    "SELECT s.ServiceId, s.ProviderId, s.CategoryId, c.Name AS CategoryName, s.Title, s.Description, " &
                    "s.Price, s.District, s.Phone, s.Latitude, s.Longitude, s.Status, s.RejectReason, s.CreatedAt, s.ViewCount, s.IsVerified " &
                    "FROM Services s JOIN Categories c ON c.CategoryId = s.CategoryId " & whereSql &
                    "ORDER BY s.CreatedAt DESC;", conn)
                    cmd.Parameters.AddRange(parameters.ToArray())
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            result.Add(Map(reader))
                        End While
                    End Using
                End Using
            End Using

            Return result
        End Function

        ' --- Адмін: повний CRUD над усіма оголошеннями (п.12 уточненої постановки, 2026-09-11) ---
        ' На відміну від методів вище (GetByProvider/GetById/Update), тут немає перевірки
        ' власника й обмеження за статусом — адмін бачить/редагує/видаляє будь-яке оголошення.

        ''' <summary>Усі оголошення незалежно від статусу, з контактами постачальника — для AdminServices.aspx.</summary>
        Public Shared Function GetAllForAdmin() As List(Of Service)
            Dim result As New List(Of Service)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT s.ServiceId, s.ProviderId, s.CategoryId, c.Name AS CategoryName, s.Title, s.Description, " &
                    "s.Price, s.District, s.Phone, s.Latitude, s.Longitude, s.Status, s.RejectReason, s.CreatedAt, s.ViewCount, s.IsVerified, " &
                    "u.FullName AS ProviderName, u.Email AS ProviderEmail " &
                    "FROM Services s " &
                    "JOIN Categories c ON c.CategoryId = s.CategoryId " &
                    "JOIN Users u ON u.UserId = s.ProviderId " &
                    "ORDER BY s.CreatedAt DESC;", conn)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim svc = Map(reader)
                            svc.ProviderName = reader.GetString("ProviderName")
                            svc.ProviderEmail = reader.GetString("ProviderEmail")
                            result.Add(svc)
                        End While
                    End Using
                End Using
            End Using
            Return result
        End Function

        ''' <summary>Оголошення за Id незалежно від власника/статусу — для адмінського редагування. Nothing, якщо не існує.</summary>
        Public Shared Function GetByIdAny(serviceId As Integer) As Service
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT s.ServiceId, s.ProviderId, s.CategoryId, c.Name AS CategoryName, s.Title, s.Description, " &
                    "s.Price, s.District, s.Phone, s.Latitude, s.Longitude, s.Status, s.RejectReason, s.CreatedAt, s.ViewCount, s.IsVerified, " &
                    "u.FullName AS ProviderName, u.Email AS ProviderEmail " &
                    "FROM Services s " &
                    "JOIN Categories c ON c.CategoryId = s.CategoryId " &
                    "JOIN Users u ON u.UserId = s.ProviderId " &
                    "WHERE s.ServiceId = @ServiceId;", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    Using reader = cmd.ExecuteReader()
                        If Not reader.Read() Then Return Nothing
                        Dim svc = Map(reader)
                        svc.ProviderName = reader.GetString("ProviderName")
                        svc.ProviderEmail = reader.GetString("ProviderEmail")
                        Return svc
                    End Using
                End Using
            End Using
        End Function

        ''' <summary>Повне редагування адміном — без перевірки власника й статусу (на відміну від Update
        ''' постачальника), дозволяє напряму змінити й Status/RejectReason.</summary>
        Public Shared Function AdminUpdate(serviceId As Integer, categoryId As Integer, title As String, description As String,
                                            price As Decimal?, district As String, phone As String,
                                            latitude As Decimal?, longitude As Decimal?,
                                            status As String, rejectReason As String, isVerified As Boolean) As Boolean
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "UPDATE Services SET CategoryId = @CategoryId, Title = @Title, Description = @Description, " &
                    "Price = @Price, District = @District, Phone = @Phone, Latitude = @Latitude, Longitude = @Longitude, " &
                    "Status = @Status, RejectReason = @RejectReason, IsVerified = @IsVerified " &
                    "WHERE ServiceId = @ServiceId;", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    cmd.Parameters.AddWithValue("@CategoryId", categoryId)
                    cmd.Parameters.AddWithValue("@Title", title)
                    cmd.Parameters.AddWithValue("@Description", If(String.IsNullOrWhiteSpace(description), DBNull.Value, CObj(description)))
                    cmd.Parameters.AddWithValue("@Price", If(price.HasValue, CObj(price.Value), DBNull.Value))
                    cmd.Parameters.AddWithValue("@District", If(String.IsNullOrWhiteSpace(district), DBNull.Value, CObj(district)))
                    cmd.Parameters.AddWithValue("@Phone", If(String.IsNullOrWhiteSpace(phone), DBNull.Value, CObj(phone)))
                    cmd.Parameters.AddWithValue("@Latitude", If(latitude.HasValue, CObj(latitude.Value), DBNull.Value))
                    cmd.Parameters.AddWithValue("@Longitude", If(longitude.HasValue, CObj(longitude.Value), DBNull.Value))
                    cmd.Parameters.AddWithValue("@Status", status)
                    cmd.Parameters.AddWithValue("@RejectReason", If(String.IsNullOrWhiteSpace(rejectReason), DBNull.Value, CObj(rejectReason)))
                    cmd.Parameters.AddWithValue("@IsVerified", isVerified)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        ''' <summary>Остаточне видалення оголошення адміном (на відміну від Unpublish постачальника —
        ''' не "зняти з публікації", а видалити рядок повністю). ServicePhotos/ModerationLog/Favorites/
        ''' Reviews/Messages прибираються каскадно (ON DELETE CASCADE, schema.sql) — тут лише БД,
        ''' файли фото на диску видаляє сторінка (AdminServices.aspx.vb), знявши шляхи до цього виклику.</summary>
        Public Shared Function AdminDelete(serviceId As Integer) As Boolean
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand("DELETE FROM Services WHERE ServiceId = @ServiceId;", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        ''' <summary>Видалення фото адміном — без перевірки власника (на відміну від DeletePhoto постачальника).
        ''' Повертає шлях файлу для видалення з диска (або Nothing, якщо фото не знайдено).</summary>
        Public Shared Function AdminDeletePhoto(photoId As Integer) As String
            Using conn = DbHelper.GetConnection()
                Dim filePath As String = Nothing
                Using selectCmd As New MySqlCommand("SELECT FilePath FROM ServicePhotos WHERE PhotoId = @PhotoId;", conn)
                    selectCmd.Parameters.AddWithValue("@PhotoId", photoId)
                    Dim result = selectCmd.ExecuteScalar()
                    If result Is Nothing Then Return Nothing
                    filePath = CStr(result)
                End Using

                Using deleteCmd As New MySqlCommand("DELETE FROM ServicePhotos WHERE PhotoId = @PhotoId;", conn)
                    deleteCmd.Parameters.AddWithValue("@PhotoId", photoId)
                    deleteCmd.ExecuteNonQuery()
                End Using

                Return filePath
            End Using
        End Function

    End Class

End Namespace
