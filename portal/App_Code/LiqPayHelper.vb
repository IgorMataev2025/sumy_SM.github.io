Imports System
Imports System.Configuration
Imports System.Globalization
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.RegularExpressions

Namespace SumyPortal

    ''' <summary>
    ''' Донат-модуль через LiqPay (уточнена постановка від 2026-09-07, п.1 —
    ''' "переказ коштів на підтримку проєкту"). Реалізує підпис/перевірку за
    ''' офіційним алгоритмом LiqPay (checkout API v3):
    ''' signature = base64(sha1(private_key + base64(json) + private_key)).
    ''' Ключі — з LiqPay.config (Web.config, appSettings file=, поза git,
    ''' див. LiqPay.config.example). Без зовнішньої JSON-бібліотеки (проєкт —
    ''' Website Project без NuGet-restore) — формат запиту фіксований, тож
    ''' будуємо/розбираємо JSON вручну (String.Format / Regex).
    ''' </summary>
    Public Class LiqPayHelper

        Private Shared ReadOnly Property PublicKey As String
            Get
                Return ConfigurationManager.AppSettings("LiqPayPublicKey")
            End Get
        End Property

        Private Shared ReadOnly Property PrivateKey As String
            Get
                Return ConfigurationManager.AppSettings("LiqPayPrivateKey")
            End Get
        End Property

        ''' <summary>Будує data/signature для форми чекауту (POST на https://www.liqpay.ua/api/3/checkout).</summary>
        Public Shared Sub BuildCheckout(orderId As String, amount As Decimal, description As String,
                                         resultUrl As String, serverUrl As String,
                                         ByRef data As String, ByRef signature As String)
            Dim json = "{" &
                """public_key"":""" & JsonEscape(PublicKey) & """," &
                """version"":""3""," &
                """action"":""pay""," &
                """amount"":""" & amount.ToString("0.00", CultureInfo.InvariantCulture) & """," &
                """currency"":""UAH""," &
                """description"":""" & JsonEscape(description) & """," &
                """order_id"":""" & JsonEscape(orderId) & """," &
                """result_url"":""" & JsonEscape(resultUrl) & """," &
                """server_url"":""" & JsonEscape(serverUrl) & """" &
                "}"

            data = Convert.ToBase64String(Encoding.UTF8.GetBytes(json))
            signature = Sign(data)
        End Sub

        ''' <summary>Підпис за алгоритмом LiqPay: base64(sha1(private_key + data + private_key)).</summary>
        Public Shared Function Sign(data As String) As String
            Using sha1 As SHA1 = SHA1.Create()
                Dim bytes = Encoding.UTF8.GetBytes(PrivateKey & data & PrivateKey)
                Dim hash = sha1.ComputeHash(bytes)
                Return Convert.ToBase64String(hash)
            End Using
        End Function

        ''' <summary>Перевіряє підпис callback-даних від LiqPay (server_url) — постійний час порівняння.</summary>
        Public Shared Function VerifySignature(data As String, signature As String) As Boolean
            If String.IsNullOrEmpty(data) OrElse String.IsNullOrEmpty(signature) Then Return False
            Return FixedTimeEquals(Sign(data), signature)
        End Function

        ''' <summary>Розкодовує base64 payload callback-у у вихідний JSON-рядок.</summary>
        Public Shared Function DecodeData(data As String) As String
            Return Encoding.UTF8.GetString(Convert.FromBase64String(data))
        End Function

        ''' <summary>Дістає значення поля з плоского JSON-об'єкта LiqPay (рядкове чи числове).</summary>
        Public Shared Function ExtractField(json As String, fieldName As String) As String
            Dim stringMatch = Regex.Match(json, """" & Regex.Escape(fieldName) & """\s*:\s*""((?:\\.|[^""\\])*)""")
            If stringMatch.Success Then
                Return stringMatch.Groups(1).Value.Replace("\""", """").Replace("\\", "\")
            End If

            Dim numericMatch = Regex.Match(json, """" & Regex.Escape(fieldName) & """\s*:\s*([0-9.eE+-]+)")
            If numericMatch.Success Then Return numericMatch.Groups(1).Value

            Return Nothing
        End Function

        Private Shared Function FixedTimeEquals(a As String, b As String) As Boolean
            If a Is Nothing OrElse b Is Nothing OrElse a.Length <> b.Length Then Return False
            Dim diff As Integer = 0
            For i = 0 To a.Length - 1
                diff = diff Or (Convert.ToInt32(a(i)) Xor Convert.ToInt32(b(i)))
            Next
            Return diff = 0
        End Function

        Private Shared Function JsonEscape(value As String) As String
            If value Is Nothing Then Return ""
            Return value.Replace("\", "\\").Replace("""", "\""")
        End Function

    End Class

End Namespace
