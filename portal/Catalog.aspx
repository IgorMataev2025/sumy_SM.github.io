<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Catalog.aspx.vb" Inherits="SumyPortal.Catalog" %>
<%@ MasterType VirtualPath="~/Site.master" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    <asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_Heading %>" /> — <asp:Literal runat="server" Text="<%$ Resources:SiteText, SiteTitleSuffix %>" />
</asp:Content>
<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Leaflet + OpenStreetMap — той самий безкоштовний стек, що вже на ServiceDetails.aspx/
         ServiceEdit.aspx (без API-ключа), тепер і для перемикача "Карта" тут (продовження
         геолокації, п.13, постановка робочої тестової версії, 2026-09-12). -->
    <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"
        integrity="sha256-p4NxAoJBhIIN+hmNHrzRCf9tD/miZyoHS5obTRR9BMY=" crossorigin="" />
    <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"
        integrity="sha256-20nQCchB9co0qIjJZRGuk2/Z9VM+kNiyxNV1lvTlZBo=" crossorigin=""></script>
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

    <!-- Перемикач Список/Карта (продовження геолокації, п.13, постановка робочої тестової
         версії, 2026-09-12) — звичайні кнопки без постбеку, JS лише показує/ховає вже
         відрендерені сервером панелі; hidViewMode зберігає стан через постбек (пошук/скидання
         фільтрів не скидають активну вкладку). -->
    <div class="catalog-view-toggle">
        <button type="button" id="btnViewList" class="btn-secondary view-toggle-btn"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_View_List %>" /></button>
        <!-- Перемикач "Таблиця" (наступна фіча понад MVP, 2026-09-14) — той самий принцип, що
             Список/Карта: сервер рендерить обидві панелі одразу (rptCatalog і rptCatalogTable —
             однакові дані, BindResults), JS лише показує/ховає. Сортування по стовпцях — суто
             клієнтське (перевпорядковує вже наявні <tr> поточної сторінки за data-атрибутами),
             без окремого запиту на сервер; не замінює серверне "Сортування" вище (те визначає,
             які САМЕ 12 оголошень потрапляють на сторінку). -->
        <button type="button" id="btnViewTable" class="btn-secondary view-toggle-btn"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_View_Table %>" /></button>
        <button type="button" id="btnViewMap" class="btn-secondary view-toggle-btn"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_View_Map %>" /></button>
    </div>
    <asp:HiddenField ID="hidViewMode" runat="server" Value="list" />

    <asp:Panel ID="listViewPanel" runat="server">
        <asp:Label ID="pageInfoLiteral" runat="server" CssClass="page-info" />

        <asp:Panel ID="emptyPanel" runat="server" Visible="false" CssClass="stub-note">
            <asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_Empty %>" />
        </asp:Panel>

        <div class="catalog-grid">
            <asp:Repeater ID="rptCatalog" runat="server">
                <ItemTemplate>
                    <a class="catalog-card" href='<%#: "ServiceDetails.aspx?id=" & CType(Container.DataItem, SumyPortal.Service).ServiceId %>'>
                        <div class="catalog-thumb">
                            <asp:Image runat="server" Visible='<%#: Not String.IsNullOrEmpty(CType(Container.DataItem, SumyPortal.Service).ThumbnailUrl) %>'
                                ImageUrl='<%#: If(String.IsNullOrEmpty(CType(Container.DataItem, SumyPortal.Service).ThumbnailUrl), "", ResolveUrl(CType(Container.DataItem, SumyPortal.Service).ThumbnailUrl)) %>' AlternateText='<%#: CType(Container.DataItem, SumyPortal.Service).Title %>' />
                            <span class="catalog-thumb-placeholder" runat="server" visible='<%#: String.IsNullOrEmpty(CType(Container.DataItem, SumyPortal.Service).ThumbnailUrl) %>'><asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_Photo_Placeholder %>" /></span>
                        </div>
                        <div class="catalog-card-body">
                            <h3><%#: CType(Container.DataItem, SumyPortal.Service).Title %></h3>
                            <!-- Позначка "Перевірено адміном" (п.22, наступна фіча понад MVP,
                                 2026-09-12) — додатковий сигнал довіри, не заміняє статус. -->
                            <asp:Label runat="server" CssClass="verified-badge"
                                Visible='<%#: CType(Container.DataItem, SumyPortal.Service).IsVerified %>'
                                Text="<%$ Resources:SiteText, Catalog_VerifiedBadge %>" />
                            <!-- Бейдж "Новинка" (наступна фіча понад MVP, 2026-09-13) — CreatedAt
                                 не старіший за NewServiceThreshold (Web.config NewServiceDays). -->
                            <asp:Label runat="server" CssClass="new-badge"
                                Visible='<%#: CType(Container.DataItem, SumyPortal.Service).CreatedAt >= NewServiceThreshold %>'
                                Text="<%$ Resources:SiteText, Catalog_NewBadge %>" />
                            <p class="service-category">
                                <%#: CType(Container.DataItem, SumyPortal.Service).CategoryName %>
                                <%#: If(String.IsNullOrEmpty(CType(Container.DataItem, SumyPortal.Service).District), "", " · " & CType(Container.DataItem, SumyPortal.Service).District) %>
                            </p>
                            <p class="catalog-price">
                                <%#: If(CType(Container.DataItem, SumyPortal.Service).Price.HasValue, CType(Container.DataItem, SumyPortal.Service).Price.Value.ToString("0.## грн"), Resources.SiteText.Price_Negotiable) %>
                            </p>
                        </div>
                    </a>
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <div class="pagination">
            <asp:LinkButton ID="lnkPrev" runat="server" OnClick="lnkPrev_Click" CausesValidation="false" Text="<%$ Resources:SiteText, Catalog_Pagination_Prev %>" />
            <asp:LinkButton ID="lnkNext" runat="server" OnClick="lnkNext_Click" CausesValidation="false" Text="<%$ Resources:SiteText, Catalog_Pagination_Next %>" />
        </div>
    </asp:Panel>

    <asp:Panel ID="tableViewPanel" runat="server" Style="display:none;">
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
                                <td><%#: CType(Container.DataItem, SumyPortal.Service).Title %></td>
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

    <asp:Panel ID="mapViewPanel" runat="server" Style="display:none;">
        <div id="catalogMap" style="height:500px; border-radius:8px;"></div>
        <asp:Panel ID="mapEmptyPanel" runat="server" Visible="false" CssClass="stub-note">
            <asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_Map_Empty %>" />
        </asp:Panel>
    </asp:Panel>

    <!-- JSON з даними для міток — рендериться сервером щоразу, коли оновлюються результати
         (пошук/скидання/перше завантаження), окремо від пагінованого списку (BindMapData,
         Catalog.aspx.vb). application/json, а не inline-змінна — JS сам не виконується як
         скрипт, безпечніше при довільному тексті назви/категорії. -->
    <script type="application/json" id="catalogMapData"><asp:Literal ID="mapDataLiteral" runat="server" /></script>

    <script>
        (function () {
            var listPanel = document.getElementById('<%= listViewPanel.ClientID %>');
            var tablePanel = document.getElementById('<%= tableViewPanel.ClientID %>');
            var mapPanel = document.getElementById('<%= mapViewPanel.ClientID %>');
            var hidViewMode = document.getElementById('<%= hidViewMode.ClientID %>');
            var btnList = document.getElementById('btnViewList');
            var btnTable = document.getElementById('btnViewTable');
            var btnMap = document.getElementById('btnViewMap');
            var detailsLinkText = <%= DetailsLinkTextForScript %>;
            var map = null;
            var markersLayer = null;

            function escapeHtml(s) {
                var div = document.createElement('div');
                div.textContent = s;
                return div.innerHTML;
            }

            function initMap() {
                if (map) return;
                var dataEl = document.getElementById('catalogMapData');
                var items = [];
                try { items = JSON.parse(dataEl.textContent || '[]'); } catch (e) { items = []; }

                // setView ДО addTo(tileLayer) — Leaflet вимагає, щоб у карти вже була
                // задана область перегляду перед додаванням шарів (інакше tileLayer не
                // може порахувати, які тайли завантажувати). Далі renderMarkers сам
                // підганяє область під фактичні мітки (fitBounds).
                map = L.map('catalogMap').setView([50.9077, 34.7981], 12); // центр Сум за замовчуванням
                L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                    attribution: '&copy; OpenStreetMap contributors',
                    maxZoom: 19
                }).addTo(map);
                markersLayer = L.layerGroup().addTo(map);

                renderMarkers(items);
            }

            function renderMarkers(items) {
                markersLayer.clearLayers();
                var bounds = [];
                items.forEach(function (item) {
                    var marker = L.marker([item.lat, item.lng]);
                    var priceText = (item.price === null) ? '' : (item.price + ' грн');
                    var popupHtml = '<b>' + escapeHtml(item.title) + '</b><br>' +
                        escapeHtml(item.category) +
                        (priceText ? '<br>' + escapeHtml(priceText) : '') +
                        '<br><a href="ServiceDetails.aspx?id=' + item.id + '">' + escapeHtml(detailsLinkText) + '</a>';
                    marker.bindPopup(popupHtml);
                    marker.addTo(markersLayer);
                    bounds.push([item.lat, item.lng]);
                });

                if (bounds.length > 0) {
                    map.fitBounds(bounds, { padding: [30, 30], maxZoom: 15 });
                }
            }

            function showView(mode) {
                hidViewMode.value = mode;
                listPanel.style.display = (mode === 'list') ? '' : 'none';
                tablePanel.style.display = (mode === 'table') ? '' : 'none';
                mapPanel.style.display = (mode === 'map') ? '' : 'none';
                btnList.classList.toggle('view-toggle-active', mode === 'list');
                btnTable.classList.toggle('view-toggle-active', mode === 'table');
                btnMap.classList.toggle('view-toggle-active', mode === 'map');
                if (mode === 'map') {
                    initMap();
                    setTimeout(function () { if (map) map.invalidateSize(); }, 0);
                }
            }

            btnList.addEventListener('click', function () { showView('list'); });
            btnTable.addEventListener('click', function () { showView('table'); });
            btnMap.addEventListener('click', function () { showView('map'); });

            showView(hidViewMode.value === 'map' ? 'map' : (hidViewMode.value === 'table' ? 'table' : 'list'));

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
