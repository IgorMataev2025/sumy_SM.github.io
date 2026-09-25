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

        ''' <summary>Специфікація (наступна фіча понад MVP, реалізовано за прямим запитом
        ''' користувача, 2026-09-14) — відносний шлях до файлу .xls/.xlsx (як ThumbnailUrl/
        ''' ServicePhoto.FilePath: "~/Uploads/Services/{id}/..."), Nothing якщо не завантажено.
        ''' Вміст рендериться SpecificationReader.TryReadAsHtmlTable(), не тут.</summary>
        Public Property SpecificationFilePath As String

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

        ''' <summary>Дата автозняття опублікованого оголошення (міграція 021) — заповнюється лише
        ''' для MyServices.aspx і листа-попередження, окремим вузьким запитом (GetExpiryDates /
        ''' WarnExpiringApproved), Map(reader) її не знає.</summary>
        Public Property ExpiresAt As DateTime?

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
                .IsVerified = reader.GetBoolean("IsVerified"),
                .SpecificationFilePath = If(reader.IsDBNull(reader.GetOrdinal("SpecificationFilePath")), Nothing, reader.GetString("SpecificationFilePath"))
            }
        End Function

        ''' <summary>
        ''' Map() читає Latitude/Longitude/ViewCount/IsVerified/SpecificationFilePath безумовно
        ''' (reader.GetOrdinal/GetInt32/GetBoolean) — тому будь-який SELECT, переданий у
        ''' Map(reader), ОБОВ'ЯЗКОВО має містити ці стовпці, інакше IndexOutOfRangeException.
        ''' Стосується й інших inline-запитів нижче (GetPendingForModeration/GetPendingById/
        ''' GetApprovedById/GetForMessaging/GetAllForAdmin/GetByIdAny/SearchApproved/
        ''' SearchApprovedForMap), що дублюють цей самий список колонок — так само Favorite.vb:
        ''' GetByUser (та сама збірка App_Code). GetApprovedForSitemap — виняток: свідомо
        ''' вузький SELECT лише ServiceId/ApprovedAt, Map(reader) там не використовується.
        ''' </summary>
        Private Const SelectBase As String =
            "SELECT s.ServiceId, s.ProviderId, s.CategoryId, c.Name AS CategoryName, s.Title, s.Description, " &
            "s.Price, s.District, s.Phone, s.Latitude, s.Longitude, s.Status, s.RejectReason, s.CreatedAt, " &
            "s.ViewCount, s.IsVerified, s.SpecificationFilePath " &
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

        ''' <summary>Швидке редагування ОПУБЛІКОВАНОГО оголошення (2026-09-24, рішення користувача
        ''' «дрібні поля без модерації») — лише ціна/район/телефон, статус лишається Approved.
        ''' Назва/опис/категорія/фото — як і раніше лише через зняття з публікації й модерацію.
        ''' Власника й статус перевіряє сам SQL (той самий прийом, що Update/Unpublish).</summary>
        Public Shared Function UpdatePublishedDetails(serviceId As Integer, providerId As Integer,
                                                      price As Decimal?, district As String, phone As String) As Boolean
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "UPDATE Services SET Price = @Price, District = @District, Phone = @Phone " &
                    "WHERE ServiceId = @ServiceId AND ProviderId = @ProviderId AND Status = 'Approved';", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    cmd.Parameters.AddWithValue("@ProviderId", providerId)
                    cmd.Parameters.AddWithValue("@Price", If(price.HasValue, CObj(price.Value), DBNull.Value))
                    cmd.Parameters.AddWithValue("@District", If(String.IsNullOrWhiteSpace(district), DBNull.Value, CObj(district)))
                    cmd.Parameters.AddWithValue("@Phone", If(String.IsNullOrWhiteSpace(phone), DBNull.Value, CObj(phone)))
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

        ''' <summary>Видалення власного оголошення постачальником (2026-09-24) — лише Draft/Rejected
        ''' (опубліковане спершу знімається з публікації, на модерації — чекає рішення адміна).
        ''' Власника й статус перевіряє сам SQL, той самий прийом, що Unpublish. Пов'язані рядки
        ''' (фото, відгуки, переписка, вподобане тощо) видаляються каскадно (ON DELETE CASCADE);
        ''' файли з диска прибирає сторінка.</summary>
        Public Shared Function Delete(serviceId As Integer, providerId As Integer) As Boolean
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "DELETE FROM Services " &
                    "WHERE ServiceId = @ServiceId AND ProviderId = @ProviderId AND Status IN ('Draft', 'Rejected');", conn)
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

        ''' <summary>«Зробити головним» (2026-09-24). Головне фото скрізь — перше за PhotoId
        ''' (GetPhotos: мініатюри каталогу/схожих, перше в галереї), тож без нового стовпця й
        ''' міграції просто міняємо FilePath обраного фото й першого місцями (в транзакції).
        ''' Лише Draft/Rejected власника — той самий обсяг, що решта змін фото в ServiceEdit.</summary>
        Public Shared Function SetMainPhoto(photoId As Integer, providerId As Integer) As Boolean
            Using conn = DbHelper.GetConnection()
                Dim serviceId As Integer
                Dim chosenPath As String
                Using cmd As New MySqlCommand(
                    "SELECT p.ServiceId, p.FilePath FROM ServicePhotos p JOIN Services s ON s.ServiceId = p.ServiceId " &
                    "WHERE p.PhotoId = @PhotoId AND s.ProviderId = @ProviderId AND s.Status IN ('Draft', 'Rejected');", conn)
                    cmd.Parameters.AddWithValue("@PhotoId", photoId)
                    cmd.Parameters.AddWithValue("@ProviderId", providerId)
                    Using reader = cmd.ExecuteReader()
                        If Not reader.Read() Then Return False
                        serviceId = reader.GetInt32("ServiceId")
                        chosenPath = reader.GetString("FilePath")
                    End Using
                End Using

                Dim firstId As Integer
                Dim firstPath As String
                Using cmd As New MySqlCommand("SELECT PhotoId, FilePath FROM ServicePhotos WHERE ServiceId = @ServiceId ORDER BY PhotoId LIMIT 1;", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    Using reader = cmd.ExecuteReader()
                        reader.Read()
                        firstId = reader.GetInt32("PhotoId")
                        firstPath = reader.GetString("FilePath")
                    End Using
                End Using
                If firstId = photoId Then Return True

                Using tx = conn.BeginTransaction()
                    For Each pair In {New KeyValuePair(Of Integer, String)(firstId, chosenPath), New KeyValuePair(Of Integer, String)(photoId, firstPath)}
                        Using cmd As New MySqlCommand("UPDATE ServicePhotos SET FilePath = @FilePath WHERE PhotoId = @PhotoId;", conn, tx)
                            cmd.Parameters.AddWithValue("@FilePath", pair.Value)
                            cmd.Parameters.AddWithValue("@PhotoId", pair.Key)
                            cmd.ExecuteNonQuery()
                        End Using
                    Next
                    tx.Commit()
                End Using
                Return True
            End Using
        End Function

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

        ' --- Специфікація (наступна фіча понад MVP, реалізовано за прямим запитом
        ' користувача, 2026-09-14) — рівно 0 або 1 файл на оголошення, тому простий
        ' стовпець Services.SpecificationFilePath, а не окрема таблиця (як ServicePhotos,
        ' де файлів кілька). ---

        ''' <summary>Поточний шлях файлу специфікації — для заміни/видалення при збереженні
        ''' (ServiceEdit.aspx.vb: ProcessSpecificationUpload). Nothing, якщо не завантажено
        ''' або оголошення не належить цьому постачальнику.</summary>
        Public Shared Function GetSpecificationFilePath(serviceId As Integer, providerId As Integer) As String
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT SpecificationFilePath FROM Services WHERE ServiceId = @ServiceId AND ProviderId = @ProviderId;", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    cmd.Parameters.AddWithValue("@ProviderId", providerId)
                    Dim result = cmd.ExecuteScalar()
                    If result Is Nothing OrElse result Is DBNull.Value Then Return Nothing
                    Return CStr(result)
                End Using
            End Using
        End Function

        ''' <summary>Записує (filePath заповнено) або очищує (Nothing — після видалення позначеним
        ''' чекбоксом) шлях файлу специфікації. Той самий принцип обмеження статусу, що Update —
        ''' редагування лише поки Draft/Rejected. Повертає False, якщо не власник/не той статус.</summary>
        Public Shared Function SetSpecificationFilePath(serviceId As Integer, providerId As Integer, filePath As String) As Boolean
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "UPDATE Services SET SpecificationFilePath = @FilePath " &
                    "WHERE ServiceId = @ServiceId AND ProviderId = @ProviderId AND Status IN ('Draft', 'Rejected');", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    cmd.Parameters.AddWithValue("@ProviderId", providerId)
                    cmd.Parameters.AddWithValue("@FilePath", If(String.IsNullOrEmpty(filePath), DBNull.Value, CObj(filePath)))
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        ' --- Модерація (адміністратор, ТЗ п.4.3) ---

        ''' <summary>Черга "на модерації", найстаріші спершу (FIFO) — з даними постачальника для контексту адміна.</summary>
        Public Shared Function GetPendingForModeration() As List(Of Service)
            Dim result As New List(Of Service)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT s.ServiceId, s.ProviderId, s.CategoryId, c.Name AS CategoryName, s.Title, s.Description, " &
                    "s.Price, s.District, s.Phone, s.Latitude, s.Longitude, s.Status, s.RejectReason, s.CreatedAt, s.ViewCount, s.IsVerified, s.SpecificationFilePath, " &
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
                    "s.Price, s.District, s.Phone, s.Latitude, s.Longitude, s.Status, s.RejectReason, s.CreatedAt, s.ViewCount, s.IsVerified, s.SpecificationFilePath, " &
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
                    "ApprovedAt = UTC_TIMESTAMP(), ApprovedBy = @AdminId, RenewedAt = NULL, ExpiryWarnedAt = NULL " &
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

        ''' <summary>Лист-попередження за warnDays до автозняття (2026-09-24, міграція 021) —
        ''' повертає оголошення, яким час попередити, і одразу позначає ExpiryWarnedAt, щоб
        ''' наступна щоденна перевірка не надіслала лист повторно (скидається при продовженні
        ''' й повторному схваленні). Той самий стиль «SELECT, потім UPDATE по рядку», що
        ''' ArchiveStaleApproved нижче.</summary>
        Public Shared Function WarnExpiringApproved(days As Integer, warnDays As Integer) As List(Of Service)
            Dim result As New List(Of Service)
            Using conn = DbHelper.GetConnection()
                Using selectCmd As New MySqlCommand(
                    "SELECT s.ServiceId, s.Title, u.FullName AS ProviderName, u.Email AS ProviderEmail, " &
                    "DATE_ADD(COALESCE(s.RenewedAt, s.ApprovedAt), INTERVAL @Days DAY) AS ExpiresAt " &
                    "FROM Services s JOIN Users u ON u.UserId = s.ProviderId " &
                    "WHERE s.Status = 'Approved' AND s.ExpiryWarnedAt IS NULL " &
                    "AND COALESCE(s.RenewedAt, s.ApprovedAt) < DATE_SUB(UTC_TIMESTAMP(), INTERVAL @WarnAfter DAY);", conn)
                    selectCmd.Parameters.AddWithValue("@Days", days)
                    selectCmd.Parameters.AddWithValue("@WarnAfter", days - warnDays)
                    Using reader = selectCmd.ExecuteReader()
                        While reader.Read()
                            result.Add(New Service With {
                                .ServiceId = reader.GetInt32("ServiceId"),
                                .Title = reader.GetString("Title"),
                                .ProviderName = reader.GetString("ProviderName"),
                                .ProviderEmail = reader.GetString("ProviderEmail"),
                                .ExpiresAt = reader.GetDateTime("ExpiresAt")
                            })
                        End While
                    End Using
                End Using

                For Each svc In result
                    Using updateCmd As New MySqlCommand(
                        "UPDATE Services SET ExpiryWarnedAt = UTC_TIMESTAMP() WHERE ServiceId = @ServiceId AND ExpiryWarnedAt IS NULL;", conn)
                        updateCmd.Parameters.AddWithValue("@ServiceId", svc.ServiceId)
                        updateCmd.ExecuteNonQuery()
                    End Using
                Next
            End Using
            Return result
        End Function

        ''' <summary>«Продовжити публікацію» постачальником (2026-09-24) — строк рахується заново
        ''' від сьогодні. ApprovedAt не чіпаємо навмисно (дайджест/sitemap/«Новинка» не повинні
        ''' вважати продовжене оголошення новим). Власника й статус перевіряє сам SQL.</summary>
        Public Shared Function Renew(serviceId As Integer, providerId As Integer) As Boolean
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "UPDATE Services SET RenewedAt = UTC_TIMESTAMP(), ExpiryWarnedAt = NULL " &
                    "WHERE ServiceId = @ServiceId AND ProviderId = @ProviderId AND Status = 'Approved';", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    cmd.Parameters.AddWithValue("@ProviderId", providerId)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        ''' <summary>Дати автозняття опублікованих оголошень постачальника — для MyServices.aspx
        ''' (вузький запит, той самий принцип, що GetApprovedForSitemap: не тягнути нове поле
        ''' в Map(reader)).</summary>
        Public Shared Function GetExpiryDates(providerId As Integer, days As Integer) As Dictionary(Of Integer, DateTime)
            Dim result As New Dictionary(Of Integer, DateTime)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT ServiceId, DATE_ADD(COALESCE(RenewedAt, ApprovedAt), INTERVAL @Days DAY) AS ExpiresAt " &
                    "FROM Services WHERE ProviderId = @ProviderId AND Status = 'Approved' AND ApprovedAt IS NOT NULL;", conn)
                    cmd.Parameters.AddWithValue("@ProviderId", providerId)
                    cmd.Parameters.AddWithValue("@Days", days)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            result(reader.GetInt32("ServiceId")) = reader.GetDateTime("ExpiresAt")
                        End While
                    End Using
                End Using
            End Using
            Return result
        End Function

        ''' <summary>Пороги автозняття з Web.config (appSettings StaleServiceDays/StaleWarningDays)
        ''' — спільні для Global.asax.vb (щоденна перевірка) і MyServices.aspx.vb (показ дати).</summary>
        Public Shared ReadOnly Property StaleDays As Integer
            Get
                Dim days As Integer
                If Not Integer.TryParse(System.Configuration.ConfigurationManager.AppSettings("StaleServiceDays"), days) Then days = 90
                Return days
            End Get
        End Property

        Public Shared ReadOnly Property StaleWarningDays As Integer
            Get
                Dim days As Integer
                If Not Integer.TryParse(System.Configuration.ConfigurationManager.AppSettings("StaleWarningDays"), days) Then days = 7
                Return days
            End Get
        End Property

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
                    "WHERE s.Status = 'Approved' AND COALESCE(s.RenewedAt, s.ApprovedAt) < DATE_SUB(UTC_TIMESTAMP(), INTERVAL @Days DAY);", conn)
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

        ''' <summary>Розширене SEO (2026-09-22) — публічні профілі постачальників
        ''' (Profile.aspx?providerId=X, п.13) для sitemap.xml: лише ті, хто має хоча б одне
        ''' Approved-оголошення (інакше сторінка публічна, але порожня — не варта індексації).
        ''' DISTINCT, той самий вузький принцип, що GetApprovedForSitemap вище.</summary>
        Public Shared Function GetApprovedProviderIdsForSitemap() As List(Of Integer)
            Dim result As New List(Of Integer)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand("SELECT DISTINCT ProviderId FROM Services WHERE Status = 'Approved';", conn)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            result.Add(reader.GetInt32("ProviderId"))
                        End While
                    End Using
                End Using
            End Using
            Return result
        End Function

        ''' <summary>Автопідказки в пошуку (п.26, наступна фіча понад MVP, 2026-09-12) —
        ''' до limit унікальних назв опублікованих оголошень (за алфавітом) для HTML5
        ''' &lt;datalist&gt; на полі "Ключове слово" (Catalog.aspx) — нативний браузерний
        ''' автокомпліт, без JS-бібліотек і AJAX. Той самий принцип вузького SELECT, що
        ''' GetApprovedForSitemap (п.21). ORDER BY навмисно за Title, а не CreatedAt —
        ''' MySQL 8 забороняє `SELECT DISTINCT col1 ... ORDER BY col2`, коли col2 не в
        ''' SELECT-списку (ERROR 3065, впіймано живим тестом до деплою).</summary>
        Public Shared Function GetDistinctApprovedTitles(limit As Integer) As List(Of String)
            Dim result As New List(Of String)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT DISTINCT s.Title FROM Services s " &
                    "WHERE s.Status = 'Approved' ORDER BY s.Title LIMIT @Limit;", conn)
                    cmd.Parameters.AddWithValue("@Limit", limit)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            result.Add(reader.GetString("Title"))
                        End While
                    End Using
                End Using
            End Using
            Return result
        End Function

        ''' <summary>Схожі оголошення (п.24, наступна фіча понад MVP, 2026-09-12) — інші
        ''' Approved-оголошення тієї ж категорії, крім самого поточного, найновіші перші,
        ''' без фолбеку на інші категорії/райони (свідоме MVP-спрощення). Той самий принцип
        ''' вузького SELECT, що GetApprovedForSitemap (п.21) — повертає Service лише з полями,
        ''' потрібними для картки (ServiceId/Title/Price/District/CategoryName); решта полів
        ''' лишаються дефолтними, Map(reader)/SelectBase тут навмисно не використовуються.</summary>
        Public Shared Function GetSimilar(categoryId As Integer, excludeServiceId As Integer, limit As Integer) As List(Of Service)
            Dim result As New List(Of Service)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT s.ServiceId, s.Title, s.Price, s.District, c.Name AS CategoryName " &
                    "FROM Services s JOIN Categories c ON c.CategoryId = s.CategoryId " &
                    "WHERE s.Status = 'Approved' AND s.CategoryId = @CategoryId AND s.ServiceId <> @ExcludeServiceId " &
                    "ORDER BY s.CreatedAt DESC LIMIT @Limit;", conn)
                    cmd.Parameters.AddWithValue("@CategoryId", categoryId)
                    cmd.Parameters.AddWithValue("@ExcludeServiceId", excludeServiceId)
                    cmd.Parameters.AddWithValue("@Limit", limit)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            result.Add(New Service With {
                                .ServiceId = reader.GetInt32("ServiceId"),
                                .Title = reader.GetString("Title"),
                                .Price = If(reader.IsDBNull(reader.GetOrdinal("Price")), CType(Nothing, Decimal?), reader.GetDecimal("Price")),
                                .District = If(reader.IsDBNull(reader.GetOrdinal("District")), Nothing, reader.GetString("District")),
                                .CategoryName = reader.GetString("CategoryName")
                            })
                        End While
                    End Using
                End Using
            End Using
            Return result
        End Function

        ''' <summary>Легка модель одного пункту дайджесту на email (п.25, наступна фіча
        ''' понад MVP, 2026-09-12) — окремий клас, а не Service, щоб не додавати ApprovedAt
        ''' як публічну властивість Service, яку довелось би тримати синхронізованою в усіх
        ''' SELECT/Map(reader) місцях (той самий "грабельний" список, що вже для Latitude/
        ''' ViewCount/IsVerified).</summary>
        Public Class DigestListing
            Public Property ServiceId As Integer
            Public Property Title As String
            Public Property CategoryName As String
            Public Property ApprovedAt As DateTime
        End Class

        ''' <summary>Усі опубліковані оголошення з датою публікації — для дайджесту на email
        ''' (п.25). Один спільний запит замість окремого SELECT на кожного адресата: кожен
        ''' користувач має власне "з якого часу" (LastDigestSentAt/CreatedAt), тож фільтрація
        ''' "що саме нове для нього" відбувається в пам'яті, у DigestSender.vb.</summary>
        Public Shared Function GetAllApprovedForDigest() As List(Of DigestListing)
            Dim result As New List(Of DigestListing)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT s.ServiceId, s.Title, c.Name AS CategoryName, s.ApprovedAt " &
                    "FROM Services s JOIN Categories c ON c.CategoryId = s.CategoryId " &
                    "WHERE s.Status = 'Approved' ORDER BY s.ApprovedAt DESC;", conn)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            result.Add(New DigestListing With {
                                .ServiceId = reader.GetInt32("ServiceId"),
                                .Title = reader.GetString("Title"),
                                .CategoryName = reader.GetString("CategoryName"),
                                .ApprovedAt = reader.GetDateTime("ApprovedAt")
                            })
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
                    "s.Price, s.District, s.Phone, s.Latitude, s.Longitude, s.Status, s.RejectReason, s.CreatedAt, s.ViewCount, s.IsVerified, s.SpecificationFilePath, " &
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
                    "s.Price, s.District, s.Phone, s.Latitude, s.Longitude, s.Status, s.RejectReason, s.CreatedAt, s.ViewCount, s.IsVerified, s.SpecificationFilePath, " &
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

        ''' <summary>Один рядок "Столу замовлень" (нова консолідуюча фіча, 2026-09-15) — одна
        ''' категорія+район+постачальник з кількістю опублікованих оголошень, що відповідають
        ''' вільнотекстовому запиту USER. Окремий клас, а не Service, бо це вже агрегований
        ''' рядок (COUNT), а не конкретне оголошення — той самий принцип, що DigestListing.</summary>
        Public Class OrderBoardRow
            Public Property CategoryId As Integer
            Public Property CategoryName As String
            Public Property District As String
            Public Property ProviderId As Integer
            Public Property ProviderName As String
            Public Property ServiceCount As Integer
        End Class

        ''' <summary>"Стіл замовлень" (нова консолідуюча фіча, 2026-09-15, ТЗ уточнюється й далі) —
        ''' USER описує потребу вільним текстом, результат — не картки оголошень (як Catalog.aspx),
        ''' а зведена картина ринку: скільки постачальників у якій категорії/районі відповідають
        ''' запиту. Порожній keyword — увесь ринок без фільтра (огляд "що є в принципі"). Контекстний
        ''' пошук (2026-09-16): keyword звіряється не лише з Title/Description оголошення, а й із
        ''' назвою категорії, районом, ПІБ і назвою компанії постачальника — щоб запит на кшталт
        ''' "Роменський" чи "ФОП Іваненко" теж знаходив відповідні рядки. Лише Approved і лише
        ''' публічні поля (без Phone) — той самий принцип видимості для анонімного USER, що вже
        ''' Catalog.aspx (п.12 уточненої моделі ролей).</summary>
        Public Shared Function SearchOrderBoard(keyword As String) As List(Of OrderBoardRow)
            Dim result As New List(Of OrderBoardRow)
            Dim sql As New StringBuilder(
                "SELECT s.CategoryId, c.Name AS CategoryName, s.District, s.ProviderId, u.FullName AS ProviderName, " &
                "COUNT(*) AS ServiceCount " &
                "FROM Services s " &
                "JOIN Categories c ON c.CategoryId = s.CategoryId " &
                "JOIN Users u ON u.UserId = s.ProviderId " &
                "WHERE s.Status = 'Approved' ")

            Dim parameters As New List(Of MySqlParameter)
            If Not String.IsNullOrWhiteSpace(keyword) Then
                sql.Append("AND (s.Title LIKE @Keyword OR s.Description LIKE @Keyword OR c.Name LIKE @Keyword " &
                            "OR s.District LIKE @Keyword OR u.FullName LIKE @Keyword OR u.CompanyName LIKE @Keyword) ")
                parameters.Add(New MySqlParameter("@Keyword", "%" & keyword & "%"))
            End If
            sql.Append("GROUP BY s.CategoryId, c.Name, s.District, s.ProviderId, u.FullName ")
            sql.Append("ORDER BY c.Name, s.District, u.FullName;")

            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(sql.ToString(), conn)
                    cmd.Parameters.AddRange(parameters.ToArray())
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            result.Add(New OrderBoardRow With {
                                .CategoryId = reader.GetInt32("CategoryId"),
                                .CategoryName = reader.GetString("CategoryName"),
                                .District = If(reader.IsDBNull(reader.GetOrdinal("District")), "", reader.GetString("District")),
                                .ProviderId = reader.GetInt32("ProviderId"),
                                .ProviderName = reader.GetString("ProviderName"),
                                .ServiceCount = reader.GetInt32("ServiceCount")
                            })
                        End While
                    End Using
                End Using
            End Using
            Return result
        End Function

        Private Shared Function BuildSearchWhere(categoryId As Integer?, district As String, keyword As String,
                                                  minPrice As Decimal?, maxPrice As Decimal?,
                                                  parameters As List(Of MySqlParameter),
                                                  Optional providerId As Integer? = Nothing) As String
            Dim sb As New StringBuilder("WHERE s.Status = 'Approved' ")

            If categoryId.HasValue Then
                sb.Append("AND s.CategoryId = @CategoryId ")
                parameters.Add(New MySqlParameter("@CategoryId", categoryId.Value))
            End If
            If providerId.HasValue Then
                sb.Append("AND s.ProviderId = @ProviderId ")
                parameters.Add(New MySqlParameter("@ProviderId", providerId.Value))
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

        ''' <summary>Безпечний ORDER BY для сортування каталогу (п.23, наступна фіча понад MVP,
        ''' 2026-09-12) — whitelist фіксованих значень з UI (Catalog.aspx: ddlSort), НЕ пряма
        ''' конкатенація довільного рядка від користувача (захист від SQL-ін'єкції). Договірна
        ''' ціна (Price IS NULL) при сортуванні за ціною завжди йде в кінець незалежно від напрямку.</summary>
        Private Shared Function BuildSortOrder(sortBy As String) As String
            Select Case sortBy
                Case "price_asc"
                    Return "ORDER BY (s.Price IS NULL), s.Price ASC "
                Case "price_desc"
                    Return "ORDER BY (s.Price IS NULL), s.Price DESC "
                Case "popular"
                    Return "ORDER BY s.ViewCount DESC "
                Case Else ' "new" і будь-яке невідоме значення — безпечний дефолт
                    Return "ORDER BY s.CreatedAt DESC "
            End Select
        End Function

        ''' <summary>Пошук опублікованих оголошень з фільтрами, сортуванням і пагінацією (ТЗ, розділ 4.4).</summary>
        Public Shared Function SearchApproved(categoryId As Integer?, district As String, keyword As String,
                                               minPrice As Decimal?, maxPrice As Decimal?, sortBy As String,
                                               pageNumber As Integer, pageSize As Integer,
                                               ByRef totalCount As Integer,
                                               Optional providerId As Integer? = Nothing) As List(Of Service)
            Dim result As New List(Of Service)

            Using conn = DbHelper.GetConnection()
                Dim countParams As New List(Of MySqlParameter)
                Dim whereSql = BuildSearchWhere(categoryId, district, keyword, minPrice, maxPrice, countParams, providerId)

                Using countCmd As New MySqlCommand("SELECT COUNT(*) FROM Services s " & whereSql & ";", conn)
                    countCmd.Parameters.AddRange(countParams.ToArray())
                    totalCount = Convert.ToInt32(countCmd.ExecuteScalar())
                End Using

                ' MySqlParameter не можна повторно прив'язати до іншої команди — будуємо WHERE ще раз зі свіжими параметрами.
                Dim selectParams As New List(Of MySqlParameter)
                Dim whereSql2 = BuildSearchWhere(categoryId, district, keyword, minPrice, maxPrice, selectParams, providerId)

                Using cmd As New MySqlCommand(
                    "SELECT s.ServiceId, s.ProviderId, s.CategoryId, c.Name AS CategoryName, s.Title, s.Description, " &
                    "s.Price, s.District, s.Phone, s.Latitude, s.Longitude, s.Status, s.RejectReason, s.CreatedAt, s.ViewCount, s.IsVerified, s.SpecificationFilePath, " &
                    "u.FullName AS ProviderName " &
                    "FROM Services s JOIN Categories c ON c.CategoryId = s.CategoryId " &
                    "JOIN Users u ON u.UserId = s.ProviderId " & whereSql2 &
                    BuildSortOrder(sortBy) & "LIMIT @PageSize OFFSET @Offset;", conn)
                    cmd.Parameters.AddRange(selectParams.ToArray())
                    cmd.Parameters.AddWithValue("@PageSize", pageSize)
                    cmd.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            ' ProviderName — для колонки "Постачальник" у табличному режимі Catalog.aspx
                            ' (2026-09-25); те саме ім'я (FullName), що показує ServiceDetails.aspx.
                            Dim svc = Map(reader)
                            svc.ProviderName = reader.GetString("ProviderName")
                            result.Add(svc)
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
                                                      minPrice As Decimal?, maxPrice As Decimal?,
                                                      Optional providerId As Integer? = Nothing) As List(Of Service)
            Dim result As New List(Of Service)

            Using conn = DbHelper.GetConnection()
                Dim parameters As New List(Of MySqlParameter)
                Dim whereSql = BuildSearchWhere(categoryId, district, keyword, minPrice, maxPrice, parameters, providerId)
                whereSql &= "AND s.Latitude IS NOT NULL AND s.Longitude IS NOT NULL "

                Using cmd As New MySqlCommand(
                    "SELECT s.ServiceId, s.ProviderId, s.CategoryId, c.Name AS CategoryName, s.Title, s.Description, " &
                    "s.Price, s.District, s.Phone, s.Latitude, s.Longitude, s.Status, s.RejectReason, s.CreatedAt, s.ViewCount, s.IsVerified, s.SpecificationFilePath " &
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
                    "s.Price, s.District, s.Phone, s.Latitude, s.Longitude, s.Status, s.RejectReason, s.CreatedAt, s.ViewCount, s.IsVerified, s.SpecificationFilePath, " &
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
                    "s.Price, s.District, s.Phone, s.Latitude, s.Longitude, s.Status, s.RejectReason, s.CreatedAt, s.ViewCount, s.IsVerified, s.SpecificationFilePath, " &
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
                    "UPDATE Services SET " &
                    "ApprovedAt = IF(@Status = 'Approved' AND Status <> 'Approved', UTC_TIMESTAMP(), ApprovedAt), " &
                    "RenewedAt = IF(@Status = 'Approved' AND Status <> 'Approved', NULL, RenewedAt), " &
                    "ExpiryWarnedAt = IF(@Status = 'Approved' AND Status <> 'Approved', NULL, ExpiryWarnedAt), " &
                    "CategoryId = @CategoryId, Title = @Title, Description = @Description, " &
                    "Price = @Price, District = @District, Phone = @Phone, Latitude = @Latitude, Longitude = @Longitude, " &
                    "Status = @Status, RejectReason = @RejectReason, IsVerified = @IsVerified " &
                    "WHERE ServiceId = @ServiceId;", conn)
                    ' Пряма публікація адміном (статус стає Approved) = те саме, що Approve(): строк
                    ' публікації починається заново. Без цього ніколи не схвалене оголошення лишалось
                    ' з ApprovedAt = NULL (падали дайджест/sitemap — reader.GetDateTime на NULL), а
                    ' раніше автознняте — з давньою датою (наступна перевірка знову б його зняла).
                    ' Присвоєння ApprovedAt/RenewedAt/ExpiryWarnedAt стоять ДО Status: MySQL виконує
                    ' SET зліва направо, тож тут Status — ще старе значення.
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
