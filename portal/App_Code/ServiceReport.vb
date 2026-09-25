Imports System
Imports System.Collections.Generic
Imports MySql.Data.MySqlClient

Namespace SumyPortal

    ''' <summary>
    ''' Скарга на оголошення (наступна фіча понад MVP, обрано автономно циклом
    ''' /loop, 2026-09-13) — будь-хто, включно з анонімом (ServiceDetails.aspx
    ''' відкрита для USER, п.12), може поскаржитись на оголошення з фіксованою
    ''' причиною і необов'язковим коментарем. Не автоматична модерація — лише
    ''' сигнал адміну (AdminReports.aspx), Status оголошення не змінюється сам
    ''' по собі. ReporterEmail — Nothing для анонімного скаржника.
    ''' </summary>
    Public Class ServiceReport
        Public Property ReportId As Integer
        Public Property ServiceId As Integer
        Public Property ServiceTitle As String
        Public Property ReporterEmail As String
        Public Property Reason As String
        Public Property Comment As String
        Public Property Status As String
        Public Property CreatedAt As DateTime

        ''' <summary>Текст причини для показу адміну — фіксований whitelist значень з
        ''' UI (ServiceDetails.aspx: ddlReportReason), той самий принцип, що
        ''' Service.BuildSortOrder (захист від довільного тексту в цьому полі).</summary>
        Public ReadOnly Property ReasonLabel As String
            Get
                Select Case Reason
                    Case "FalseInfo" : Return "Неправдива інформація"
                    Case "Fraud" : Return "Шахрайство / обман"
                    Case "Prohibited" : Return "Заборонений контент"
                    Case "Duplicate" : Return "Дублікат оголошення"
                    Case Else : Return "Інше"
                End Select
            End Get
        End Property

        Public Shared Sub Add(serviceId As Integer, reporterEmail As String, reason As String, comment As String)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "INSERT INTO ServiceReports (ServiceId, ReporterEmail, Reason, Comment, CreatedAt) " &
                    "VALUES (@ServiceId, @ReporterEmail, @Reason, @Comment, UTC_TIMESTAMP());", conn)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    cmd.Parameters.AddWithValue("@ReporterEmail", If(String.IsNullOrWhiteSpace(reporterEmail), DBNull.Value, CObj(reporterEmail)))
                    cmd.Parameters.AddWithValue("@Reason", reason)
                    cmd.Parameters.AddWithValue("@Comment", If(String.IsNullOrWhiteSpace(comment), DBNull.Value, CObj(comment.Trim())))
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        End Sub

        ''' <summary>Відкриті скарги, найстаріші спершу (FIFO, той самий принцип, що черга
        ''' модерації Service.GetPendingForModeration) — з назвою оголошення для контексту.</summary>
        Public Shared Function GetOpen() As List(Of ServiceReport)
            Dim result As New List(Of ServiceReport)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT r.ReportId, r.ServiceId, s.Title AS ServiceTitle, r.ReporterEmail, r.Reason, r.Comment, r.Status, r.CreatedAt " &
                    "FROM ServiceReports r JOIN Services s ON s.ServiceId = r.ServiceId " &
                    "WHERE r.Status = 'Open' ORDER BY r.CreatedAt ASC;", conn)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            result.Add(New ServiceReport With {
                                .ReportId = reader.GetInt32("ReportId"),
                                .ServiceId = reader.GetInt32("ServiceId"),
                                .ServiceTitle = reader.GetString("ServiceTitle"),
                                .ReporterEmail = If(reader.IsDBNull(reader.GetOrdinal("ReporterEmail")), Nothing, reader.GetString("ReporterEmail")),
                                .Reason = reader.GetString("Reason"),
                                .Comment = If(reader.IsDBNull(reader.GetOrdinal("Comment")), Nothing, reader.GetString("Comment")),
                                .Status = reader.GetString("Status"),
                                .CreatedAt = reader.GetDateTime("CreatedAt")
                            })
                        End While
                    End Using
                End Using
            End Using
            Return result
        End Function

        ''' <summary>Позначає скаргу переглянутою адміном. Повертає False, якщо скарга не існує.</summary>
        Public Shared Function MarkReviewed(reportId As Integer, adminId As Integer) As Boolean
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "UPDATE ServiceReports SET Status = 'Reviewed', ReviewedAt = UTC_TIMESTAMP(), ReviewedBy = @AdminId " &
                    "WHERE ReportId = @ReportId;", conn)
                    cmd.Parameters.AddWithValue("@AdminId", adminId)
                    cmd.Parameters.AddWithValue("@ReportId", reportId)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        ''' <summary>Відкриті скарги, згруповані за оголошенням (2026-09-25, аудит Адміна, п.3) —
        ''' кілька скарг на одне оголошення = одна картка з лічильником; найстаріша скарга
        ''' групи визначає порядок (FIFO, як і раніше).</summary>
        Public Shared Function GetOpenGrouped() As List(Of ServiceReportGroup)
            Dim groups As New List(Of ServiceReportGroup)
            Dim byService As New Dictionary(Of Integer, ServiceReportGroup)
            Dim statuses As New Dictionary(Of Integer, String)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT DISTINCT s.ServiceId, s.Status FROM ServiceReports r JOIN Services s ON s.ServiceId = r.ServiceId WHERE r.Status = 'Open';", conn)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            statuses(reader.GetInt32("ServiceId")) = reader.GetString("Status")
                        End While
                    End Using
                End Using
            End Using
            For Each report In GetOpen()
                Dim group As ServiceReportGroup = Nothing
                If Not byService.TryGetValue(report.ServiceId, group) Then
                    group = New ServiceReportGroup With {
                        .ServiceId = report.ServiceId,
                        .ServiceTitle = report.ServiceTitle,
                        .ServiceStatus = If(statuses.ContainsKey(report.ServiceId), statuses(report.ServiceId), "")
                    }
                    byService(report.ServiceId) = group
                    groups.Add(group)
                End If
                group.Reports.Add(report)
            Next
            Return groups
        End Function

        ''' <summary>Закриває всі відкриті скарги на оголошення (після рішення адміна по групі).</summary>
        Public Shared Function MarkReviewedForService(serviceId As Integer, adminId As Integer) As Integer
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "UPDATE ServiceReports SET Status = 'Reviewed', ReviewedAt = UTC_TIMESTAMP(), ReviewedBy = @AdminId " &
                    "WHERE ServiceId = @ServiceId AND Status = 'Open';", conn)
                    cmd.Parameters.AddWithValue("@AdminId", adminId)
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId)
                    Return cmd.ExecuteNonQuery()
                End Using
            End Using
        End Function

    End Class

    Public Class ServiceReportGroup
        Public Property ServiceId As Integer
        Public Property ServiceTitle As String
        Public Property ServiceStatus As String
        Public Property Reports As New List(Of ServiceReport)

        Public ReadOnly Property IsPublished As Boolean
            Get
                Return ServiceStatus = "Approved"
            End Get
        End Property
    End Class

End Namespace
