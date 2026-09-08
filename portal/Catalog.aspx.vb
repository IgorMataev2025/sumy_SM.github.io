Imports System
Imports System.Globalization

Namespace SumyPortal

    Public Class Catalog
        Inherits System.Web.UI.Page

        Private Const PageSize As Integer = 12

        Private Property CurrentPage As Integer
            Get
                Return If(ViewState("CurrentPage"), 1)
            End Get
            Set(value As Integer)
                ViewState("CurrentPage") = value
            End Set
        End Property

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not IsPostBack Then
                BindFilterOptions()
                PreselectCategoryFromQueryString()
                CurrentPage = 1
                BindResults()
            End If
        End Sub

        ''' <summary>Перехід із карток категорій на головній (Default.aspx?...→Catalog.aspx?categoryId=X).</summary>
        Private Sub PreselectCategoryFromQueryString()
            Dim categoryId As Integer
            If Integer.TryParse(Request.QueryString("categoryId"), categoryId) Then
                Dim item = ddlCategory.Items.FindByValue(categoryId.ToString())
                If item IsNot Nothing Then ddlCategory.SelectedValue = categoryId.ToString()
            End If
        End Sub

        Private Sub BindFilterOptions()
            ddlCategory.Items.Clear()
            ddlCategory.Items.Add(New ListItem("Всі категорії", "0"))
            For Each cat In ServiceCategory.GetActiveCategories()
                ddlCategory.Items.Add(New ListItem(cat.Name, cat.CategoryId.ToString()))
            Next

            ' Фіксований довідник районів (п.5 уточненої постановки), а не
            ' лише ті, де вже є оголошення — щоб фільтр завжди показував усі
            ' офіційні райони.
            SumyDistricts.Populate(ddlDistrict, "Всі райони")
        End Sub

        Private Sub BindResults()
            Dim categoryId As Integer? = Nothing
            Dim parsedCategoryId As Integer
            If Integer.TryParse(ddlCategory.SelectedValue, parsedCategoryId) AndAlso parsedCategoryId > 0 Then
                categoryId = parsedCategoryId
            End If

            Dim district = If(String.IsNullOrEmpty(ddlDistrict.SelectedValue), Nothing, ddlDistrict.SelectedValue)
            Dim keyword = txtKeyword.Text.Trim()

            Dim minPrice As Decimal? = ParsePrice(txtMinPrice.Text)
            Dim maxPrice As Decimal? = ParsePrice(txtMaxPrice.Text)

            Dim total As Integer
            Dim items = Service.SearchApproved(categoryId, district, keyword, minPrice, maxPrice, CurrentPage, PageSize, total)

            ' Захист від виходу за межі (напр. якщо дані змінились між запитами) — повертаємось на останню сторінку.
            Dim totalPagesCheck = Math.Max(1, CInt(Math.Ceiling(total / CDbl(PageSize))))
            If CurrentPage > totalPagesCheck Then
                CurrentPage = totalPagesCheck
                items = Service.SearchApproved(categoryId, district, keyword, minPrice, maxPrice, CurrentPage, PageSize, total)
            End If

            For Each item In items
                Dim photos = Service.GetPhotos(item.ServiceId)
                If photos.Count > 0 Then item.ThumbnailUrl = photos(0).FilePath
            Next

            rptCatalog.DataSource = items
            rptCatalog.DataBind()
            emptyPanel.Visible = (items.Count = 0)

            Dim totalPages = Math.Max(1, CInt(Math.Ceiling(total / CDbl(PageSize))))
            pageInfoLiteral.Text = String.Format("Знайдено: {0}. Сторінка {1} з {2}.", total, CurrentPage, totalPages)
            lnkPrev.Enabled = (CurrentPage > 1)
            lnkNext.Enabled = (CurrentPage < totalPages)
        End Sub

        Private Function ParsePrice(text As String) As Decimal?
            If String.IsNullOrWhiteSpace(text) Then Return Nothing
            Dim value As Decimal
            If Decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, value) Then Return value
            Return Nothing
        End Function

        Protected Sub btnSearch_Click(sender As Object, e As EventArgs)
            If Not Page.IsValid Then Return
            CurrentPage = 1
            BindResults()
        End Sub

        Protected Sub btnReset_Click(sender As Object, e As EventArgs)
            ddlCategory.SelectedIndex = 0
            ddlDistrict.SelectedIndex = 0
            txtKeyword.Text = String.Empty
            txtMinPrice.Text = String.Empty
            txtMaxPrice.Text = String.Empty
            CurrentPage = 1
            BindResults()
        End Sub

        Protected Sub lnkPrev_Click(sender As Object, e As EventArgs)
            If CurrentPage > 1 Then CurrentPage -= 1
            BindResults()
        End Sub

        Protected Sub lnkNext_Click(sender As Object, e As EventArgs)
            CurrentPage += 1
            BindResults()
        End Sub

    End Class

End Namespace
