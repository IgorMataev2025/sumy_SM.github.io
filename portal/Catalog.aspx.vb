Imports System
Imports System.Configuration
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Web

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

        ''' <summary>Фільтр за постачальником (перехід з OrderBoard.aspx "Оголошення →",
        ''' 2026-09-23) — немає власного UI-контролу (на відміну від категорії/району), тому
        ''' переживає постбеки (пошук/сортування/пагінація) через ViewState, той самий підхід,
        ''' що CurrentPage. Скидається лише "Скинути"/переходом на чистий Catalog.aspx.</summary>
        Private Property ProviderIdFilter As Integer?
            Get
                If ViewState("ProviderIdFilter") Is Nothing Then Return Nothing
                Return CType(ViewState("ProviderIdFilter"), Integer)
            End Get
            Set(value As Integer?)
                ViewState("ProviderIdFilter") = value
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
            Master.OgTitle = "Каталог послуг — Портал послуг Safina"

            If Not IsPostBack Then
                BindFilterOptions()
                BindKeywordSuggestions()
                PreselectCategoryFromQueryString()
                PreselectProviderFromQueryString()
                PreselectFiltersFromQueryString()
                BindResults()
            End If
        End Sub

        ''' <summary>Стан каталогу в адресі (2026-09-25): кожна дія (пошук/сортування/
        ''' сторінка/скидання) робить Response.Redirect на Catalog.aspx?… (PRG), тож
        ''' "← До каталогу" і кнопка "Назад" браузера повертають той самий список, а не
        ''' скинутий. Тут — зворотне: з query string у контроли. Невідомий район/сорт
        ''' просто ігнорується (FindByValue), ціни — як ввів користувач (ParsePrice далі).</summary>
        Private Sub PreselectFiltersFromQueryString()
            Dim district = Request.QueryString("district")
            If Not String.IsNullOrEmpty(district) AndAlso ddlDistrict.Items.FindByValue(district) IsNot Nothing Then
                ddlDistrict.SelectedValue = district
            End If
            txtKeyword.Text = If(Request.QueryString("q"), String.Empty)
            txtMinPrice.Text = If(Request.QueryString("min"), String.Empty)
            txtMaxPrice.Text = If(Request.QueryString("max"), String.Empty)
            Dim sort = Request.QueryString("sort")
            If Not String.IsNullOrEmpty(sort) AndAlso ddlSort.Items.FindByValue(sort) IsNot Nothing Then
                ddlSort.SelectedValue = sort
            End If
            Dim page As Integer
            CurrentPage = If(Integer.TryParse(Request.QueryString("page"), page) AndAlso page > 1, page, 1)
        End Sub

        ''' <summary>Адреса каталогу з поточними фільтрами — лише непорожні параметри, щоб
        ''' посилання лишались короткими (Catalog.aspx без фільтрів = просто Catalog.aspx).</summary>
        Private Function BuildCatalogUrl(page As Integer) As String
            Dim parts As New List(Of String)
            Dim add = Sub(key As String, value As String)
                          If Not String.IsNullOrWhiteSpace(value) Then parts.Add(key & "=" & HttpUtility.UrlEncode(value.Trim()))
                      End Sub
            If ddlCategory.SelectedValue <> "0" Then add("categoryId", ddlCategory.SelectedValue)
            add("district", ddlDistrict.SelectedValue)
            add("q", txtKeyword.Text)
            add("min", txtMinPrice.Text)
            add("max", txtMaxPrice.Text)
            If ddlSort.SelectedIndex > 0 Then add("sort", ddlSort.SelectedValue)
            If ProviderIdFilter.HasValue Then add("providerId", ProviderIdFilter.Value.ToString())
            If page > 1 Then add("page", page.ToString())
            Return "~/Catalog.aspx" & If(parts.Count > 0, "?" & String.Join("&", parts), "")
        End Function

        Private Sub RedirectToPage(page As Integer)
            Response.Redirect(BuildCatalogUrl(page), False)
            Context.ApplicationInstance.CompleteRequest()
        End Sub

        ''' <summary>Перехід зі "Стола замовлень" (OrderBoard.aspx "Оголошення →", 2026-09-23) —
        ''' на відміну від categoryId тут немає dropdown для вибору, тому фільтр застосовується
        ''' мовчки (немає що "переобрати" в UI) і показується окремим банером
        ''' (providerFilterPanel) з посиланням скинути. Невідомий/чужий providerId просто дає
        ''' порожній каталог (Service.SearchApproved і так фільтрує лише Approved), без винятку.</summary>
        Private Sub PreselectProviderFromQueryString()
            Dim providerId As Integer
            If Integer.TryParse(Request.QueryString("providerId"), providerId) Then
                Dim provider = UserAccount.GetById(providerId)
                If provider IsNot Nothing Then
                    ProviderIdFilter = providerId
                    providerFilterPanel.Visible = True
                    providerFilterNameLiteral.Text = Server.HtmlEncode(
                        If(provider.IsLegalEntity AndAlso Not String.IsNullOrWhiteSpace(provider.CompanyName),
                           provider.CompanyName, provider.FullName))
                End If
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
            Dim items = Service.SearchApproved(categoryId, district, keyword, minPrice, maxPrice, sortBy, CurrentPage, PageSize, total, ProviderIdFilter)

            ' Захист від виходу за межі (напр. якщо дані змінились між запитами) — повертаємось на останню сторінку.
            Dim totalPagesCheck = Math.Max(1, CInt(Math.Ceiling(total / CDbl(PageSize))))
            If CurrentPage > totalPagesCheck Then
                CurrentPage = totalPagesCheck
                items = Service.SearchApproved(categoryId, district, keyword, minPrice, maxPrice, sortBy, CurrentPage, PageSize, total, ProviderIdFilter)
            End If

            ' Каталог — лише таблиця (2026-09-25): режими "Список" (картки з мініатюрами) і
            ' "Карта" (BindMapData/SearchApprovedForMap) прибрано за рішенням користувача.
            rptCatalogTable.DataSource = items
            rptCatalogTable.DataBind()
            tableEmptyPanel.Visible = (items.Count = 0)

            Dim totalPages = Math.Max(1, CInt(Math.Ceiling(total / CDbl(PageSize))))
            tablePageInfoLiteral.Text = String.Format(Resources.SiteText.Catalog_PageInfo, total, CurrentPage, totalPages)
            lnkPrevTable.Enabled = (CurrentPage > 1)
            lnkNextTable.Enabled = (CurrentPage < totalPages)
        End Sub

        Private Function ParsePrice(text As String) As Decimal?
            If String.IsNullOrWhiteSpace(text) Then Return Nothing
            Dim value As Decimal
            If Decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, value) Then Return value
            Return Nothing
        End Function

        Protected Sub btnSearch_Click(sender As Object, e As EventArgs)
            If Not Page.IsValid Then Return
            RedirectToPage(1)
        End Sub

        Protected Sub btnReset_Click(sender As Object, e As EventArgs)
            Response.Redirect("~/Catalog.aspx", False)
            Context.ApplicationInstance.CompleteRequest()
        End Sub

        ''' <summary>Сортування каталогу (п.23) — AutoPostBack на ddlSort застосовує вибір
        ''' одразу, без окремого натискання "Знайти" (рішення користувача — "динамічно").</summary>
        Protected Sub ddlSort_SelectedIndexChanged(sender As Object, e As EventArgs)
            RedirectToPage(1)
        End Sub

        Protected Sub lnkPrev_Click(sender As Object, e As EventArgs)
            RedirectToPage(Math.Max(1, CurrentPage - 1))
        End Sub

        Protected Sub lnkNext_Click(sender As Object, e As EventArgs)
            RedirectToPage(CurrentPage + 1)
        End Sub

    End Class

End Namespace
