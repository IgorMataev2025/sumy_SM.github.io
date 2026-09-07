Imports System.Configuration
Imports MySql.Data.MySqlClient

Namespace SumyPortal

    ''' <summary>
    ''' Допоміжний клас для роботи з MySQL. Рядок підключення береться з
    ''' ConnectionStrings.config (див. Web.config, configSource) — реальні
    ''' логін/пароль поза репозиторієм.
    ''' Нефункціональна вимога ТЗ (розділ 8): лише параметризовані запити.
    ''' </summary>
    Public NotInheritable Class DbHelper

        Private Sub New()
        End Sub

        Private Shared ReadOnly ConnStr As String =
            ConfigurationManager.ConnectionStrings("SumyPortalDb").ConnectionString

        ''' <summary>Повертає відкрите з'єднання. Викликач відповідає за Dispose (Using).</summary>
        Public Shared Function GetConnection() As MySqlConnection
            Dim conn As New MySqlConnection(ConnStr)
            conn.Open()
            Return conn
        End Function

    End Class

End Namespace
