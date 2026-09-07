Imports System.Collections.Generic
Imports MySql.Data.MySqlClient

Namespace SumyPortal

    ''' <summary>
    ''' Модель категорії послуг. Дані читаються з таблиці Categories (MySQL,
    ''' ТЗ п.7) — етап 1.2. Керування (додати/редагувати/деактивувати) —
    ''' етап 4, ТЗ п.4.3.
    ''' </summary>
    Public Class ServiceCategory
        Public Property CategoryId As Integer
        Public Property Name As String
        Public Property Description As String
        Public Property IsActive As Boolean

        Public Sub New(categoryId As Integer, name As String, description As String, Optional isActive As Boolean = True)
            Me.CategoryId = categoryId
            Me.Name = name
            Me.Description = description
            Me.IsActive = isActive
        End Sub

        ''' <summary>Активні категорії з БД, у порядку CategoryId (ТЗ, розділ 3) — для форм реєстрації/оголошень.</summary>
        Public Shared Function GetActiveCategories() As List(Of ServiceCategory)
            Return Query("WHERE IsActive = 1 ")
        End Function

        ''' <summary>Усі категорії, включно з деактивованими — для адмін-панелі.</summary>
        Public Shared Function GetAllCategories() As List(Of ServiceCategory)
            Return Query("")
        End Function

        Private Shared Function Query(whereClause As String) As List(Of ServiceCategory)
            Dim result As New List(Of ServiceCategory)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "SELECT CategoryId, Name, Description, IsActive FROM Categories " & whereClause & "ORDER BY CategoryId;", conn)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            result.Add(New ServiceCategory(
                                reader.GetInt32("CategoryId"),
                                reader.GetString("Name"),
                                If(reader.IsDBNull(reader.GetOrdinal("Description")), String.Empty, reader.GetString("Description")),
                                reader.GetBoolean("IsActive")))
                        End While
                    End Using
                End Using
            End Using
            Return result
        End Function

        Public Shared Function Create(name As String, description As String) As Integer
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "INSERT INTO Categories (Name, Description, IsActive) VALUES (@Name, @Description, TRUE); " &
                    "SELECT LAST_INSERT_ID();", conn)
                    cmd.Parameters.AddWithValue("@Name", name)
                    cmd.Parameters.AddWithValue("@Description", If(String.IsNullOrWhiteSpace(description), DBNull.Value, CObj(description)))
                    Return Convert.ToInt32(cmd.ExecuteScalar())
                End Using
            End Using
        End Function

        Public Shared Function Update(categoryId As Integer, name As String, description As String) As Boolean
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "UPDATE Categories SET Name = @Name, Description = @Description WHERE CategoryId = @CategoryId;", conn)
                    cmd.Parameters.AddWithValue("@Name", name)
                    cmd.Parameters.AddWithValue("@Description", If(String.IsNullOrWhiteSpace(description), DBNull.Value, CObj(description)))
                    cmd.Parameters.AddWithValue("@CategoryId", categoryId)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        Public Shared Function SetActive(categoryId As Integer, isActive As Boolean) As Boolean
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand(
                    "UPDATE Categories SET IsActive = @IsActive WHERE CategoryId = @CategoryId;", conn)
                    cmd.Parameters.AddWithValue("@IsActive", isActive)
                    cmd.Parameters.AddWithValue("@CategoryId", categoryId)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function
    End Class

End Namespace
