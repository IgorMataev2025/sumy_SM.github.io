Imports System
Imports System.Collections.Generic
Imports System.Web
Imports System.Web.UI.WebControls

Namespace SumyPortal

    ''' <summary>
    ''' Базовий клас для сторінок адмін-панелі (ТЗ, розділ 4.3). Сторінка вже
    ''' захищена Forms-автентифікацією (Web.config, deny users="?"), тут
    ''' додатково перевіряється прапорець IsAdmin — інших не пускаємо.
    ''' </summary>
    Public MustInherit Class AdminPageBase
        Inherits System.Web.UI.Page

        Protected CurrentAdmin As UserAccount

        Protected Const AdminPageSize As Integer = 30

        Protected Overrides Sub OnInit(e As EventArgs)
            MyBase.OnInit(e)

            CurrentAdmin = UserAccount.FindByEmail(Page.User.Identity.Name)

            If CurrentAdmin Is Nothing OrElse Not CurrentAdmin.IsAdmin Then
                Response.Redirect("~/Default.aspx", True)
            End If
        End Sub

        ' --- Фільтри/пошук/сторінки в адресі (2026-09-25, аудит Адміна, п.4/5/7/9) ---
        ' Той самий PRG-підхід, що Catalog.aspx: кнопка "Знайти" робить редірект на
        ' адресу з параметрами, сторінка читає їх на GET — посилання можна зберегти,
        ' а "Назад" у браузері не питає про повторну відправку форми.

        Protected Function QueryParam(name As String) As String
            Return If(Request.QueryString(name), String.Empty).Trim()
        End Function

        Protected ReadOnly Property CurrentPageNumber As Integer
            Get
                Dim page As Integer
                Return If(Integer.TryParse(Request.QueryString("page"), page) AndAlso page > 1, page, 1)
            End Get
        End Property

        ''' <summary>Адреса поточної сторінки з параметрами (порожні пропускаються).</summary>
        Protected Function BuildQueryUrl(params As IDictionary(Of String, String)) As String
            Dim parts As New List(Of String)
            For Each kv In params
                If Not String.IsNullOrWhiteSpace(kv.Value) Then parts.Add(kv.Key & "=" & HttpUtility.UrlEncode(kv.Value.Trim()))
            Next
            Return Request.Path & If(parts.Count > 0, "?" & String.Join("&", parts), "")
        End Function

        Protected Sub RedirectWithParams(params As IDictionary(Of String, String))
            Response.Redirect(BuildQueryUrl(params), False)
            Context.ApplicationInstance.CompleteRequest()
        End Sub

        ''' <summary>"← Попередня / Наступна →" і "Знайдено: N. Сторінка X з Y" — params без "page".</summary>
        Protected Sub BindPager(prevLink As HyperLink, nextLink As HyperLink, infoLabel As Label,
                                total As Integer, params As IDictionary(Of String, String))
            Dim page = CurrentPageNumber
            Dim totalPages = Math.Max(1, CInt(Math.Ceiling(total / CDbl(AdminPageSize))))
            infoLabel.Text = String.Format("Знайдено: {0}. Сторінка {1} з {2}.", total, Math.Min(page, totalPages), totalPages)

            Dim prevParams As New Dictionary(Of String, String)(params)
            If page - 1 > 1 Then prevParams("page") = (page - 1).ToString()
            prevLink.NavigateUrl = BuildQueryUrl(prevParams)
            prevLink.Visible = (page > 1)

            Dim nextParams As New Dictionary(Of String, String)(params)
            nextParams("page") = (page + 1).ToString()
            nextLink.NavigateUrl = BuildQueryUrl(nextParams)
            nextLink.Visible = (page < totalPages)
        End Sub

    End Class

End Namespace
