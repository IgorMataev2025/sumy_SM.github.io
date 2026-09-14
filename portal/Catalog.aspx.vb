Imports System
Imports System.Configuration
Imports System.Globalization
Imports System.Text

Namespace SumyPortal

    Public Class Catalog
        Inherits System.Web.UI.Page

        Private Const PageSize As Integer = 12

        ''' <summary>Багатомовність (постановка робочої тестової версії, 2026-09-12) —
        ''' офіційна точка ASP.NET Web Forms для програмної культури, до того як
        ''' вона "застигне" на задекларованому в Web.config значенні (uk-UA).</summary>
        Protected Overrides Sub InitializeCulture()
            LocalizationHelper.ApplyCulture(Me)
            MyBase.InitializeCulture()
        End Sub

        Private Property CurrentPage As Integer
            Get
                Return If(ViewState("CurrentPage"), 1)
            End Get
            Set(value As Integer)
                ViewState("CurrentPage") = value
            End Set
        End Property

        ''' <summary>Бейдж "Новинка" (наступна фіча понад MVP, 2026-09-13) — межа за
        ''' CreatedAt (той самий стовпець, що сортування "спочатку нові", п.23; без нової
        ''' міграції БД). Той самий прийом Web.config appSettings, що StaleServiceDays/
        ''' DigestIntervalDays (Global.asax.vb).</summary>
        Protected ReadOnly Property NewServiceThreshold As DateTime
            Get
                Dim days As Integer
                If Not Integer.TryParse(ConfigurationManager.AppSettings("NewServiceDays"), days) Then days = 7
                Return DateTime.UtcNow.AddDays(-days)
            End Get
        End Property

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            ' Базове SEO (п.21, наступна фіча понад MVP, 2026-09-12) — свій опис замість
            ' загального з Site.master; на кожному запиті (не лише Not IsPostBack), бо
            ' постбек (пошук/пагінація) лишається тим самим документом для пошуковика.
            Master.MetaDescription = "Каталог послуг у Сумах та області — побутові послуги, " &
                "медицина, освіта, комунальні послуги. Фільтр за категорією, районом і ціною."

            If Not IsPostBack Then
                BindFilterOptions()
                BindKeywordSuggestions()
                PreselectCategoryFromQueryString()
                CurrentPage = 1
                BindResults()
            End If
        End Sub

        ''' <summary>Автопідказки в пошуку (п.26, наступна фіча понад MVP, 2026-09-12) —
        ''' наповнює &lt;datalist&gt; (Catalog.aspx) до 50 унікальних назв опублікованих
        ''' оголошень для нативного браузерного автокомпліту поля "Ключове слово".
        ''' Лише на першому завантаженні (Not IsPostBack) — той самий список підказок не
        ''' залежить від поточних фільтрів/сторінки, перебудовувати на кожному постбеку
        ''' не потрібно.</summary>
        Private Sub BindKeywordSuggestions()
            rptKeywordSuggestions.DataSource = Service.GetDistinctApprovedTitles(50)
            rptKeywordSuggestions.DataBind()
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
            ddlCategory.Items.Add(New ListItem(Resources.SiteText.Catalog_AllCategories, "0"))
            For Each cat In ServiceCategory.GetActiveCategories()
                ddlCategory.Items.Add(New ListItem(cat.Name, cat.CategoryId.ToString()))
            Next

            ' Фіксований довідник районів (п.5 уточненої постановки), а не
            ' лише ті, де вже є оголошення — щоб фільтр завжди показував усі
            ' офіційні райони.
            SumyDistricts.Populate(ddlDistrict, Resources.SiteText.Catalog_AllDistricts)
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

            Dim sortBy = ddlSort.SelectedValue

            Dim total As Integer
            Dim items = Service.SearchApproved(categoryId, district, keyword, minPrice, maxPrice, sortBy, CurrentPage, PageSize, total)

            ' Захист від виходу за межі (напр. якщо дані змінились між запитами) — повертаємось на останню сторінку.
            Dim totalPagesCheck = Math.Max(1, CInt(Math.Ceiling(total / CDbl(PageSize))))
            If CurrentPage > totalPagesCheck Then
                CurrentPage = totalPagesCheck
                items = Service.SearchApproved(categoryId, district, keyword, minPrice, maxPrice, sortBy, CurrentPage, PageSize, total)
            End If

            For Each item In items
                Dim photos = Service.GetPhotos(item.ServiceId)
                If photos.Count > 0 Then item.ThumbnailUrl = photos(0).FilePath
            Next

            rptCatalog.DataSource = items
            rptCatalog.DataBind()
            emptyPanel.Visible = (items.Count = 0)

            ' Перемикач "Таблиця" (наступна фіча понад MVP, 2026-09-14) — та сама сторінка
            ' даних (items), що й список карток, лише інший рендер + клієнтське сортування.
            rptCatalogTable.DataSource = items
            rptCatalogTable.DataBind()
            tableEmptyPanel.Visible = (items.Count = 0)

            Dim totalPages = Math.Max(1, CInt(Math.Ceiling(total / CDbl(PageSize))))
            pageInfoLiteral.Text = String.Format(Resources.SiteText.Catalog_PageInfo, total, CurrentPage, totalPages)
            tablePageInfoLiteral.Text = pageInfoLiteral.Text
            lnkPrev.Enabled = (CurrentPage > 1)
            lnkNext.Enabled = (CurrentPage < totalPages)
            lnkPrevTable.Enabled = lnkPrev.Enabled
            lnkNextTable.Enabled = lnkNext.Enabled

            BindMapData(categoryId, district, keyword, minPrice, maxPrice)
        End Sub

        ''' <summary>Карта каталогу (продовження геолокації, п.13, постановка робочої тестової
        ''' версії, 2026-09-12) — ті самі фільтри, що й список, але без пагінації (SearchApprovedForMap
        ''' сама відкидає оголошення без мітки). JSON рендериться в &lt;script type="application/json"&gt;
        ''' (Catalog.aspx), JS зчитує його лише коли користувач реально перемикається на вкладку "Карта".</summary>
        Private Sub BindMapData(categoryId As Integer?, district As String, keyword As String,
                                 minPrice As Decimal?, maxPrice As Decimal?)
            Dim items = Service.SearchApprovedForMap(categoryId, district, keyword, minPrice, maxPrice)

            Dim sb As New StringBuilder("[")
            For i As Integer = 0 To items.Count - 1
                Dim item = items(i)
                If i > 0 Then sb.Append(",")
                sb.Append("{""id"":").Append(item.ServiceId)
                sb.Append(",""title"":""").Append(JsonEscape(item.Title)).Append("""")
                sb.Append(",""category"":""").Append(JsonEscape(item.CategoryName)).Append("""")
                sb.Append(",""price"":").Append(If(item.Price.HasValue, item.Price.Value.ToString("0.##", CultureInfo.InvariantCulture), "null"))
                sb.Append(",""lat"":").Append(item.Latitude.Value.ToString(CultureInfo.InvariantCulture))
                sb.Append(",""lng"":").Append(item.Longitude.Value.ToString(CultureInfo.InvariantCulture))
                sb.Append("}")
            Next
            sb.Append("]")

            mapDataLiteral.Text = sb.ToString()
            mapEmptyPanel.Visible = (items.Count = 0)
        End Sub

        ''' <summary>Мінімальне власне екранування для JSON-значень, що йдуть у
        ''' &lt;script type="application/json"&gt; (без сторонніх бібліотек — той самий підхід, що
        ''' LiqPayHelper.vb для підпису). "&lt;/" екранується окремо від стандартних JSON-екранувань —
        ''' браузер закриває &lt;script&gt; за буквальним "&lt;/script" незалежно від значення type,
        ''' якщо назва/категорія випадково міститимуть такий текст.</summary>
        Private Function JsonEscape(s As String) As String
            If s Is Nothing Then Return String.Empty
            Return s.Replace("\", "\\").Replace("""", "\""").Replace(vbCr, "").Replace(vbLf, "\n").Replace("</", "<\/")
        End Function

        ''' <summary>JSON-рядок (у лапках, з екрануванням) для вставки прямо в JS-код розмітки —
        ''' той самий прийом, що LatitudeForScript/LongitudeForScript у ServiceDetails.aspx.vb
        ''' (InvariantCulture/готове значення для JS-літерала, а не сирий Resources-рядок).</summary>
        Protected ReadOnly Property DetailsLinkTextForScript As String
            Get
                Return """" & JsonEscape(Resources.SiteText.Catalog_Map_DetailsLink) & """"
            End Get
        End Property

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
            ddlSort.SelectedIndex = 0
            CurrentPage = 1
            BindResults()
        End Sub

        ''' <summary>Сортування каталогу (п.23) — AutoPostBack на ddlSort застосовує вибір
        ''' одразу, без окремого натискання "Знайти" (рішення користувача — "динамічно").</summary>
        Protected Sub ddlSort_SelectedIndexChanged(sender As Object, e As EventArgs)
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
