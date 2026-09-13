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

    End Class

End Namespace
