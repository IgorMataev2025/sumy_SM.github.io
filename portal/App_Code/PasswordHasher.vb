Imports System
Imports System.Security.Cryptography

Namespace SumyPortal

    ''' <summary>
    ''' Хешування паролів PBKDF2/SHA-256 (нефункціональна вимога ТЗ, розділ 8:
    ''' паролі не зберігаються у відкритому вигляді). Формат збереженого рядка:
    ''' "PBKDF2.&lt;ітерації&gt;.&lt;сіль base64&gt;.&lt;хеш base64&gt;" — версіонований,
    ''' щоб у майбутньому можна було підняти кількість ітерацій без міграції даних.
    ''' </summary>
    Public NotInheritable Class PasswordHasher

        Private Const Iterations As Integer = 100000
        Private Const SaltSize As Integer = 16
        Private Const HashSize As Integer = 32

        Private Sub New()
        End Sub

        Public Shared Function Hash(password As String) As String
            Dim salt(SaltSize - 1) As Byte
            Using rng = RandomNumberGenerator.Create()
                rng.GetBytes(salt)
            End Using

            Using pbkdf2 As New Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256)
                Dim hashBytes = pbkdf2.GetBytes(HashSize)
                Return String.Join(".", "PBKDF2", Iterations.ToString(), Convert.ToBase64String(salt), Convert.ToBase64String(hashBytes))
            End Using
        End Function

        Public Shared Function Verify(password As String, storedHash As String) As Boolean
            If String.IsNullOrEmpty(storedHash) Then Return False

            Dim parts = storedHash.Split("."c)
            If parts.Length <> 4 OrElse parts(0) <> "PBKDF2" Then Return False

            Dim iterations As Integer
            If Not Integer.TryParse(parts(1), iterations) Then Return False

            Dim salt = Convert.FromBase64String(parts(2))
            Dim expectedHash = Convert.FromBase64String(parts(3))

            Using pbkdf2 As New Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256)
                Dim actualHash = pbkdf2.GetBytes(expectedHash.Length)
                Return FixedTimeEquals(actualHash, expectedHash)
            End Using
        End Function

        ''' <summary>Токен для підтвердження email / скидання пароля — 32 випадкові байти у hex.</summary>
        Public Shared Function GenerateToken() As String
            Dim bytes(31) As Byte
            Using rng = RandomNumberGenerator.Create()
                rng.GetBytes(bytes)
            End Using
            Return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant()
        End Function

        ''' <summary>Порівняння за постійний час — захист від timing-атак.</summary>
        Private Shared Function FixedTimeEquals(a As Byte(), b As Byte()) As Boolean
            If a.Length <> b.Length Then Return False
            Dim diff As Integer = 0
            For i = 0 To a.Length - 1
                diff = diff Or (a(i) Xor b(i))
            Next
            Return diff = 0
        End Function

    End Class

End Namespace
