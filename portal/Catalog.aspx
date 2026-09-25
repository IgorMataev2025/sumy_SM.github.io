<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Catalog.aspx.vb" Inherits="SumyPortal.Catalog" %>
<%@ MasterType VirtualPath="~/Site.master" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    <asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_Heading %>" /> — <asp:Literal runat="server" Text="<%$ Resources:SiteText, SiteTitleSuffix %>" />
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_Heading %>" /></h1>

    <!-- Банер фільтра постачальника (перехід з OrderBoard.aspx "Оголошення →", 2026-09-23) —
         немає власного UI-контролу серед фільтрів нижче, тому мовчазний фільтр був би
         незрозумілим користувачу; банер пояснює звідки звузився список і дає посилання
         скинути (звичайний перехід на чистий Catalog.aspx, без постбеку). -->
    <asp:Panel ID="providerFilterPanel" runat="server" CssClass="stub-note" Visible="false">
        <asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_ProviderFilter_Prefix %>" />
        <b><asp:Literal ID="providerFilterNameLiteral" runat="server" /></b>
        — <a href="Catalog.aspx"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_ProviderFilter_Clear %>" /></a>
    </asp:Panel>

    <asp:Panel ID="filterPanel" runat="server" CssClass="filter-panel">
        <div class="filter-row">
            <div class="form-row">
                <label for="<%= ddlCategory.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_Label_Category %>" /></label>
                <asp:DropDownList ID="ddlCategory" runat="server" DataTextField="Name" DataValueField="CategoryId" />
            </div>
            <div class="form-row">
                <label for="<%= ddlDistrict.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_Label_District %>" /></label>
                <asp:DropDownList ID="ddlDistrict" runat="server" />
            </div>
            <div class="form-row">
                <label for="<%= txtKeyword.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_Label_Keyword %>" /></label>
                <!-- Автопідказки в пошуку (п.26, наступна фіча понад MVP, 2026-09-12) — нативний
                     HTML5 datalist, без JS-бібліотек/AJAX; список підказок нижче (keywordSuggestions). -->
                <asp:TextBox ID="txtKeyword" runat="server" placeholder="<%$ Resources:SiteText, Catalog_Keyword_Placeholder %>" list="keywordSuggestions" />
                <datalist id="keywordSuggestions">
                    <asp:Repeater ID="rptKeywordSuggestions" runat="server">
                        <ItemTemplate>
                            <option value='<%#: Container.DataItem %>' />
                        </ItemTemplate>
                    </asp:Repeater>
                </datalist>
            </div>
        </div>
        <div class="filter-row">
            <div class="form-row">
                <label for="<%= txtMinPrice.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_Label_MinPrice %>" /></label>
                <asp:TextBox ID="txtMinPrice" runat="server" MaxLength="10" />
                <asp:RegularExpressionValidator runat="server" ControlToValidate="txtMinPrice" ErrorMessage="<%$ Resources:SiteText, Catalog_Validator_Number %>" ValidationExpression="^\d+(\.\d{1,2})?$" Display="Dynamic" CssClass="field-error" />
            </div>
            <div class="form-row">
                <label for="<%= txtMaxPrice.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_Label_MaxPrice %>" /></label>
                <asp:TextBox ID="txtMaxPrice" runat="server" MaxLength="10" />
                <asp:RegularExpressionValidator runat="server" ControlToValidate="txtMaxPrice" ErrorMessage="<%$ Resources:SiteText, Catalog_Validator_Number %>" ValidationExpression="^\d+(\.\d{1,2})?$" Display="Dynamic" CssClass="field-error" />
            </div>
            <div class="form-row filter-actions">
                <asp:Button ID="btnSearch" runat="server" Text="<%$ Resources:SiteText, Catalog_Btn_Search %>" OnClick="btnSearch_Click" CssClass="btn-primary" />
                <asp:Button ID="btnReset" runat="server" Text="<%$ Resources:SiteText, Catalog_Btn_Reset %>" OnClick="btnReset_Click" CausesValidation="false" CssClass="btn-secondary" />
            </div>
        </div>
        <!-- Сортування каталогу (п.23, наступна фіча понад MVP, 2026-09-12) — окремий рядок,
             AutoPostBack застосовує вибір одразу, без "Знайти" (рішення користувача — "динамічно"). -->
        <div class="filter-row">
            <div class="form-row">
                <label for="<%= ddlSort.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_Label_Sort %>" /></label>
                <asp:DropDownList ID="ddlSort" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlSort_SelectedIndexChanged" CausesValidation="false">
                    <asp:ListItem Text="<%$ Resources:SiteText, Catalog_Sort_New %>" Value="new" />
                    <asp:ListItem Text="<%$ Resources:SiteText, Catalog_Sort_PriceAsc %>" Value="price_asc" />
                    <asp:ListItem Text="<%$ Resources:SiteText, Catalog_Sort_PriceDesc %>" Value="price_desc" />
                    <asp:ListItem Text="<%$ Resources:SiteText, Catalog_Sort_Popular %>" Value="popular" />
                </asp:DropDownList>
            </div>
        </div>
    </asp:Panel>

    <%-- Каталог — лише таблиця (рішення користувача 2026-09-25): перемикачі "Список" (картки
         з фото) і "Карта" (Leaflet-мітки) прибрано; фото й мітка лишаються на ServiceDetails.aspx.
         Сортування по стовпцях — суто клієнтське (перевпорядковує вже наявні <tr> поточної
         сторінки за data-атрибутами), не замінює серверне "Сортування" вище (те визначає,
         які САМЕ 12 оголошень потрапляють на сторінку). --%>
    <asp:Panel ID="tableViewPanel" runat="server">
        <asp:Label ID="tablePageInfoLiteral" runat="server" CssClass="page-info" />

        <asp:Panel ID="tableEmptyPanel" runat="server" Visible="false" CssClass="stub-note">
            <asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_Empty %>" />
        </asp:Panel>

        <div class="catalog-table-wrap">
            <table class="catalog-table">
                <thead>
                    <tr>
                        <th data-sort="provider" data-type="text"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_Table_Provider %>" /><span class="sort-arrow"></span></th>
                        <th data-sort="title" data-type="text"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_Table_Name %>" /><span class="sort-arrow"></span></th>
                        <th data-sort="category" data-type="text"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_Label_Category %>" /><span class="sort-arrow"></span></th>
                        <th data-sort="district" data-type="text"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_Label_District %>" /><span class="sort-arrow"></span></th>
                        <th data-sort="price" data-type="number"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_Table_Price %>" /><span class="sort-arrow"></span></th>
                        <th data-sort="date" data-type="number"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_Table_Date %>" /><span class="sort-arrow"></span></th>
                    </tr>
                </thead>
                <tbody id="catalogTableBody">
                    <asp:Repeater ID="rptCatalogTable" runat="server">
                        <ItemTemplate>
                            <tr class="catalog-table-row"
                                data-href='<%#: "ServiceDetails.aspx?id=" & CType(Container.DataItem, SumyPortal.Service).ServiceId %>'
                                data-provider='<%#: CType(Container.DataItem, SumyPortal.Service).ProviderName %>'
                                data-title='<%#: CType(Container.DataItem, SumyPortal.Service).Title %>'
                                data-category='<%#: CType(Container.DataItem, SumyPortal.Service).CategoryName %>'
                                data-district='<%#: CType(Container.DataItem, SumyPortal.Service).District %>'
                                data-price='<%#: If(CType(Container.DataItem, SumyPortal.Service).Price.HasValue, CType(Container.DataItem, SumyPortal.Service).Price.Value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture), "") %>'
                                data-date='<%#: CType(Container.DataItem, SumyPortal.Service).CreatedAt.Ticks %>'>
                                <td><%#: CType(Container.DataItem, SumyPortal.Service).ProviderName %></td>
                                <td>
                                    <%#: CType(Container.DataItem, SumyPortal.Service).Title %>
                                    <%-- Позначки "Перевірено адміном" (п.22) і "Новинка" — раніше були лише на
                                         картках режиму "Список"; перенесено сюди, коли лишилась тільки таблиця. --%>
                                    <asp:Label runat="server" CssClass="verified-badge"
                                        Visible='<%#: CType(Container.DataItem, SumyPortal.Service).IsVerified %>'
                                        Text="<%$ Resources:SiteText, Catalog_VerifiedBadge %>" />
                                    <asp:Label runat="server" CssClass="new-badge"
                                        Visible='<%#: CType(Container.DataItem, SumyPortal.Service).CreatedAt >= NewServiceThreshold %>'
                                        Text="<%$ Resources:SiteText, Catalog_NewBadge %>" />
                                </td>
                                <td><%#: CType(Container.DataItem, SumyPortal.Service).CategoryName %></td>
                                <td><%#: If(String.IsNullOrEmpty(CType(Container.DataItem, SumyPortal.Service).District), Resources.SiteText.Details_NotSpecified, CType(Container.DataItem, SumyPortal.Service).District) %></td>
                                <td><%#: If(CType(Container.DataItem, SumyPortal.Service).Price.HasValue, CType(Container.DataItem, SumyPortal.Service).Price.Value.ToString("0.## грн"), Resources.SiteText.Price_Negotiable) %></td>
                                <td><%#: CType(Container.DataItem, SumyPortal.Service).CreatedAt.ToString("dd.MM.yyyy") %></td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>
            </table>
        </div>

        <div class="pagination">
            <asp:LinkButton ID="lnkPrevTable" runat="server" OnClick="lnkPrev_Click" CausesValidation="false" Text="<%$ Resources:SiteText, Catalog_Pagination_Prev %>" />
            <asp:LinkButton ID="lnkNextTable" runat="server" OnClick="lnkNext_Click" CausesValidation="false" Text="<%$ Resources:SiteText, Catalog_Pagination_Next %>" />
        </div>
    </asp:Panel>

    <script>
        // Адреса з фільтрами (кожна дія каталогу — редірект на Catalog.aspx?…) — для
        // "← До каталогу" на ServiceDetails.aspx (2026-09-25).
        try { sessionStorage.setItem('catalogUrl', location.pathname + location.search); } catch (e) { }

        (function () {
            var tablePanel = document.getElementById('<%= tableViewPanel.ClientID %>');

            // Сортування таблиці (наступна фіча понад MVP, 2026-09-14) — суто клієнтське,
            // перевпорядковує вже наявні <tr> поточної сторінки за data-атрибутами рядка
            // (без запиту на сервер). Ціна/дата без значення завжди в кінці незалежно від
            // напрямку — той самий принцип "порожнє в кінці", що вже сортування "за ціною"
            // на сервері (Service.vb: NULL LAST).
            var tableBody = document.getElementById('catalogTableBody');
            var tableHeaders = tablePanel.querySelectorAll('th[data-sort]');
            var sortState = { column: null, dir: 1 };

            function sortTableBy(column, type) {
                var rows = Array.prototype.slice.call(tableBody.querySelectorAll('tr'));
                var dir = (sortState.column === column) ? -sortState.dir : 1;
                sortState = { column: column, dir: dir };

                rows.sort(function (a, b) {
                    var av = a.dataset[column] || '';
                    var bv = b.dataset[column] || '';
                    if (type === 'number') {
                        var an = av === '' ? null : parseFloat(av);
                        var bn = bv === '' ? null : parseFloat(bv);
                        if (an === null && bn === null) return 0;
                        if (an === null) return 1;
                        if (bn === null) return -1;
                        return (an - bn) * dir;
                    }
                    return av.localeCompare(bv, 'uk') * dir;
                });

                rows.forEach(function (row) { tableBody.appendChild(row); });

                tableHeaders.forEach(function (th) {
                    var arrow = th.querySelector('.sort-arrow');
                    arrow.textContent = (th.dataset.sort === column) ? (dir === 1 ? ' ▲' : ' ▼') : '';
                });
            }

            tableHeaders.forEach(function (th) {
                th.addEventListener('click', function () { sortTableBy(th.dataset.sort, th.dataset.type); });
            });

            tableBody.addEventListener('click', function (e) {
                var row = e.target.closest('tr[data-href]');
                if (row) window.location = row.dataset.href;
            });
        })();
    </script>
</asp:Content>
