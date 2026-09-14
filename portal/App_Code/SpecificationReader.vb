Imports System
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Web
Imports ExcelDataReader

Namespace SumyPortal

    ''' <summary>
    ''' Специфікація оголошення (наступна фіча понад MVP, реалізовано за прямим запитом
    ''' користувача, 2026-09-14) — постачальник прикріплює файл .xls/.xlsx до оголошення
    ''' (ServiceEdit.aspx), і його вміст показується як звичайна HTML-таблиця і постачальнику
    ''' (на сторінці редагування), і споживачу (ServiceDetails.aspx) — не сам файл на
    ''' завантаження/iframe, а розпарсений перегляд.
    '''
    ''' Парсинг — через ExcelDataReader (MIT, net45-збірка без сторонніх залежностей,
    ''' завантажена напряму з nuget.org і покладена в bin/ вручну — той самий принцип
    ''' "DLL у bin/ без NuGet", що вже MySql.Data.dll; підтверджено користувачем окремо
    ''' 2026-09-14 через AskUserQuestion, бо це перший випадок, коли бінарник узято не з
    ''' офіційного інсталятора постачальника, а напряму з пакетного реєстру).
    '''
    ''' Лише перший аркуш файлу (ExcelDataReader позиціюється на ньому за замовчуванням,
    ''' NextResult() свідомо не викликається) — для MVP-специфікації цього достатньо.
    ''' </summary>
    Public NotInheritable Class SpecificationReader

        Private Const MaxRows As Integer = 200
        Private Const MaxColumns As Integer = 50

        ''' <summary>Розпарсити файл у physicalPath як HTML-таблицю (перший рядок — заголовок).
        ''' False — файл пошкоджений/не Excel/порожній; html лишається Nothing, викликач сам
        ''' вирішує, що показати (той самий "не впасти на поганих вхідних даних" принцип, що
        ''' Catalog_Empty/Details_NotFound в інших місцях проєкту).</summary>
        Public Shared Function TryReadAsHtmlTable(physicalPath As String, ByRef html As String) As Boolean
            html = Nothing
            Try
                Using stream = File.Open(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read)
                    Using reader = ExcelReaderFactory.CreateReader(stream)
                        Dim sb As New StringBuilder()
                        sb.Append("<table class=""spec-table"">")

                        Dim rowIndex As Integer = 0
                        Dim wroteAnyRow As Boolean = False
                        Do While reader.Read()
                            If rowIndex >= MaxRows Then Exit Do

                            Dim colCount = Math.Min(reader.FieldCount, MaxColumns)
                            ' Порожній рядок (усі клітинки без значення) — типовий "хвіст" після
                            ' даних у файлах, збережених з великим використаним діапазоном; не
                            ' показуємо як порожній <tr>, просто пропускаємо.
                            Dim hasValue = False
                            For col As Integer = 0 To colCount - 1
                                If reader.GetValue(col) IsNot Nothing Then
                                    hasValue = True
                                    Exit For
                                End If
                            Next
                            If Not hasValue Then
                                rowIndex += 1
                                Continue Do
                            End If

                            Dim cellTag = If(rowIndex = 0, "th", "td")
                            sb.Append("<tr>")
                            For col As Integer = 0 To colCount - 1
                                sb.Append("<").Append(cellTag).Append(">")
                                sb.Append(HttpUtility.HtmlEncode(FormatCell(reader.GetValue(col))))
                                sb.Append("</").Append(cellTag).Append(">")
                            Next
                            sb.Append("</tr>")

                            wroteAnyRow = True
                            rowIndex += 1
                        Loop

                        sb.Append("</table>")

                        If Not wroteAnyRow Then Return False

                        html = sb.ToString()
                        Return True
                    End Using
                End Using
            Catch
                ' Пошкоджений файл/не Excel-формат/несподівана структура — не валимо сторінку,
                ' викликач (ServiceEdit.aspx.vb/ServiceDetails.aspx.vb) сам вирішує, що показати.
                Return False
            End Try
        End Function

        Private Shared Function FormatCell(value As Object) As String
            If value Is Nothing Then Return String.Empty
            If TypeOf value Is DateTime Then Return CType(value, DateTime).ToString("dd.MM.yyyy")
            If TypeOf value Is Double OrElse TypeOf value Is Single OrElse TypeOf value Is Decimal Then
                Return Convert.ToDecimal(value, CultureInfo.InvariantCulture).ToString("0.####", CultureInfo.InvariantCulture)
            End If
            Return Convert.ToString(value, CultureInfo.InvariantCulture)
        End Function

    End Class

End Namespace
