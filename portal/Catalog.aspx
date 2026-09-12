<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Catalog.aspx.vb" Inherits="SumyPortal.Catalog" %>
<%@ MasterType VirtualPath="~/Site.master" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Каталог послуг — Портал послуг Safina
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
                <asp:TextBox ID="txtKeyword" runat="server" placeholder="<%$ Resources:SiteText, Catalog_Keyword_Placeholder %>" />
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
    </asp:Panel>

    <!-- Перемикач Список/Карта (продовження геолокації, п.13, постановка робочої тестової
         версії, 2026-09-12) — звичайні кнопки без постбеку, JS лише показує/ховає вже
         відрендерені сервером панелі; hidViewMode зберігає стан через постбек (пошук/скидання
         фільтрів не скидають активну вкладку). -->
    <div class="catalog-view-toggle">
        <button type="button" id="btnViewList" class="btn-secondary view-toggle-btn"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_View_List %>" /></button>
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
                                ImageUrl='<%#: If(String.IsNullOrEmpty(CType(Container.DataItem, SumyPortal.Service).ThumbnailUrl), "", ResolveUrl(CType(Container.DataItem, SumyPortal.Service).ThumbnailUrl)) %>' AlternateText="" />
                            <span class="catalog-thumb-placeholder" runat="server" visible='<%#: String.IsNullOrEmpty(CType(Container.DataItem, SumyPortal.Service).ThumbnailUrl) %>'><asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_Photo_Placeholder %>" /></span>
                        </div>
                        <div class="catalog-card-body">
                            <h3><%#: CType(Container.DataItem, SumyPortal.Service).Title %></h3>
                            <!-- Позначка "Перевірено адміном" (п.22, наступна фіча понад MVP,
                                 2026-09-12) — додатковий сигнал довіри, не заміняє статус. -->
                            <asp:Label runat="server" CssClass="verified-badge"
                                Visible='<%#: CType(Container.DataItem, SumyPortal.Service).IsVerified %>'
                                Text="<%$ Resources:SiteText, Catalog_VerifiedBadge %>" />
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
            var mapPanel = document.getElementById('<%= mapViewPanel.ClientID %>');
            var hidViewMode = document.getElementById('<%= hidViewMode.ClientID %>');
            var btnList = document.getElementById('btnViewList');
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
                if (mode === 'map') {
                    listPanel.style.display = 'none';
                    mapPanel.style.display = '';
                    btnMap.classList.add('view-toggle-active');
                    btnList.classList.remove('view-toggle-active');
                    initMap();
                    setTimeout(function () { if (map) map.invalidateSize(); }, 0);
                } else {
                    listPanel.style.display = '';
                    mapPanel.style.display = 'none';
                    btnList.classList.add('view-toggle-active');
                    btnMap.classList.remove('view-toggle-active');
                }
            }

            btnList.addEventListener('click', function () { showView('list'); });
            btnMap.addEventListener('click', function () { showView('map'); });

            showView(hidViewMode.value === 'map' ? 'map' : 'list');
        })();
    </script>
</asp:Content>
