Imports System.Collections.Generic
Imports System.Web.UI.WebControls

Namespace SumyPortal

    ''' <summary>
    ''' Довідник районів Сумської області (п.5 уточненої постановки — закриває
    ''' відкрите питання ТЗ, розділ 10, "Повний перелік районів/громад
    ''' Сумської області"). Свідоме спрощення: лише 5 районів (адміністративна
    ''' реформа 2020/2021), без деталізації до громад — офіційний і незмінний
    ''' перелік, тому зберігається в коді, а не в БД (на відміну від Categories,
    ''' яку може редагувати адміністратор).
    ''' </summary>
    Public NotInheritable Class SumyDistricts

        Public Shared ReadOnly All As String() = {
            "Конотопський",
            "Охтирський",
            "Роменський",
            "Сумський",
            "Шосткинський"
        }

        ''' <summary>Заповнює DropDownList районами. За потреби додає перший пункт "усі"/"не вказано" з порожнім значенням.</summary>
        Public Shared Sub Populate(ddl As DropDownList, Optional emptyOptionText As String = Nothing)
            ddl.Items.Clear()
            If emptyOptionText IsNot Nothing Then
                ddl.Items.Add(New ListItem(emptyOptionText, String.Empty))
            End If
            For Each district In All
                ddl.Items.Add(New ListItem(district, district))
            Next
        End Sub

    End Class

End Namespace
