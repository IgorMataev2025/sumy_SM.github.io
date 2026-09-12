Imports System
Imports System.Collections.Generic
Imports MySql.Data.MySqlClient

Namespace SumyPortal

    ''' <summary>
    ''' Фото галереї на акаунті постачальника (постановка робочої тестової версії,
    ''' 2026-09-12) — окремо від ServicePhotos (фото прив'язані до оголошення), тут
    ''' фото прив'язані до самого постачальника. Публічна (Profile.aspx?providerId=X,
    ''' доступна й неавторизованим), керує лише власник. Той самий патерн методів,
    ''' що Service.vb "--- Фото ---" (GetPhotos/CountPhotos/AddPhoto/DeletePhoto).
    ''' </summary>
    Public Class ProviderGalleryPhoto
        Public Property PhotoId As Integer
        Public Property ProviderId As Integer
        Public Property FilePath As String

        Public Shared Function GetByProvider(providerId As Integer) As List(Of ProviderGalleryPhoto)
            Dim result As New List(Of ProviderGalleryPhoto)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand("SELECT PhotoId, ProviderId, FilePath FROM ProviderGalleryPhotos WHERE ProviderId = @ProviderId ORDER BY PhotoId;", conn)
                    cmd.Parameters.AddWithValue("@ProviderId", providerId)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            result.Add(New ProviderGalleryPhoto With {
                                .PhotoId = reader.GetInt32("PhotoId"),
                                .ProviderId = reader.GetInt32("ProviderId"),
                                .FilePath = reader.GetString("FilePath")
                            })
                        End While
                    End Using
                End Using
            End Using
            Return result
        End Function

        Public Shared Function Count(providerId As Integer) As Integer
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand("SELECT COUNT(*) FROM ProviderGalleryPhotos WHERE ProviderId = @ProviderId;", conn)
                    cmd.Parameters.AddWithValue("@ProviderId", providerId)
                    Return Convert.ToInt32(cmd.ExecuteScalar())
                End Using
            End Using
        End Function

        Public Shared Sub Add(providerId As Integer, filePath As String)
            Using conn = DbHelper.GetConnection()
                Using cmd As New MySqlCommand("INSERT INTO ProviderGalleryPhotos (ProviderId, FilePath) VALUES (@ProviderId, @FilePath);", conn)
                    cmd.Parameters.AddWithValue("@ProviderId", providerId)
                    cmd.Parameters.AddWithValue("@FilePath", filePath)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        End Sub

        ''' <summary>Видаляє запис фото, якщо воно належить цьому постачальнику. Повертає шлях файлу для видалення з диска (або Nothing).</summary>
        Public Shared Function Delete(photoId As Integer, providerId As Integer) As String
            Using conn = DbHelper.GetConnection()
                Dim filePath As String = Nothing
                Using selectCmd As New MySqlCommand(
                    "SELECT FilePath FROM ProviderGalleryPhotos WHERE PhotoId = @PhotoId AND ProviderId = @ProviderId;", conn)
                    selectCmd.Parameters.AddWithValue("@PhotoId", photoId)
                    selectCmd.Parameters.AddWithValue("@ProviderId", providerId)
                    Dim result = selectCmd.ExecuteScalar()
                    If result Is Nothing Then Return Nothing
                    filePath = CStr(result)
                End Using

                Using deleteCmd As New MySqlCommand("DELETE FROM ProviderGalleryPhotos WHERE PhotoId = @PhotoId;", conn)
                    deleteCmd.Parameters.AddWithValue("@PhotoId", photoId)
                    deleteCmd.ExecuteNonQuery()
                End Using

                Return filePath
            End Using
        End Function

    End Class

End Namespace
