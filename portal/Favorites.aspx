<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Favorites.aspx.vb" Inherits="SumyPortal.Favorites" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    <asp:Literal runat="server" Text="<%$ Resources:SiteText, Nav_Favorites %>" /> — <asp:Literal runat="server" Text="<%$ Resources:SiteText, SiteTitleSuffix %>" />
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Literal runat="server" Text="<%$ Resources:SiteText, Nav_Favorites %>" /></h1>

    <asp:Panel ID="emptyPanel" runat="server" Visible="false" CssClass="stub-note">
        <asp:Literal runat="server" Text="<%$ Resources:SiteText, Favorites_Empty_Prefix %>" />
        <a href="Catalog.aspx"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Favorites_Empty_CatalogLink %>" /></a>
        <asp:Literal runat="server" Text="<%$ Resources:SiteText, Favorites_Empty_Suffix %>" />
    </asp:Panel>

    <%-- 2026-09-25 (за запитом користувача): «Обране» — таблицею, як каталог, із сортуванням
         по стовпцях; клік по рядку (або ↑/↓) показує під таблицею розгорнуту інформацію,
         подвійний клік відкриває оголошення. Панелі подробиць рендеряться сервером для
         кожного рядка (прихованими) — JS лише показує потрібну, без додаткових запитів. --%>
    <asp:Panel ID="tablePanel" runat="server">
        <p class="stub-note"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Favorites_Hint %>" /></p>
        <div class="catalog-table-wrap">
            <table class="catalog-table" id="favTable">
                <thead>
                    <tr>
                        <th><asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_Table_Photo %>" /></th>
                        <th data-sort="provider" data-type="text"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_Table_Provider %>" /><span class="sort-arrow"></span></th>
                        <th data-sort="title" data-type="text"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_Table_Name %>" /><span class="sort-arrow"></span></th>
                        <th data-sort="category" data-type="text"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_Label_Category %>" /><span class="sort-arrow"></span></th>
                        <th data-sort="district" data-type="text"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_Label_District %>" /><span class="sort-arrow"></span></th>
                        <th data-sort="price" data-type="number"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_Table_Price %>" /><span class="sort-arrow"></span></th>
                        <th data-sort="rating" data-type="number"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_Table_Rating %>" /><span class="sort-arrow"></span></th>
                        <th data-sort="added" data-type="number"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Favorites_Col_Added %>" /><span class="sort-arrow"></span></th>
                    </tr>
                </thead>
                <tbody id="favTableBody" tabindex="0" style="outline:none;">
                    <asp:Repeater ID="rptRows" runat="server">
                        <ItemTemplate>
                            <tr class="catalog-table-row"
                                data-id='<%#: Item(Container).ServiceId %>'
                                data-href='<%#: If(IsAvailable(Container.DataItem), "ServiceDetails.aspx?id=" & Item(Container).ServiceId, "") %>'
                                data-provider='<%#: Item(Container).ProviderName %>'
                                data-title='<%#: Item(Container).Title %>'
                                data-category='<%#: Item(Container).CategoryName %>'
                                data-district='<%#: Item(Container).District %>'
                                data-price='<%#: If(Item(Container).Price.HasValue, Item(Container).Price.Value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture), "") %>'
                                data-rating='<%#: If(Item(Container).ReviewAverage.HasValue, Item(Container).ReviewAverage.Value.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), "") %>'
                                data-added='<%#: Item(Container).FavoritedAt.Value.Ticks %>'
                                style='<%#: If(IsAvailable(Container.DataItem), "", "opacity:0.55;") %>'>
                                <td>
                                    <asp:Image runat="server" Visible='<%#: Not String.IsNullOrEmpty(Item(Container).ThumbnailUrl) %>'
                                        ImageUrl='<%#: If(String.IsNullOrEmpty(Item(Container).ThumbnailUrl), "", ResolveUrl(Item(Container).ThumbnailUrl)) %>'
                                        AlternateText="" style="width:48px;height:36px;object-fit:cover;border-radius:4px;display:block;" />
                                </td>
                                <td><%#: Item(Container).ProviderName %></td>
                                <td>
                                    <%#: Item(Container).Title %>
                                    <asp:Label runat="server" CssClass="status-badge" Visible='<%# Not IsAvailable(Container.DataItem) %>'
                                        Text="<%$ Resources:SiteText, Favorites_Unavailable %>" />
                                </td>
                                <td><%#: Item(Container).CategoryName %></td>
                                <td><%#: If(String.IsNullOrEmpty(Item(Container).District), Resources.SiteText.Details_NotSpecified, Item(Container).District) %></td>
                                <td><%#: PriceText(Container.DataItem) %></td>
                                <td style="white-space:nowrap;"><%#: RatingText(Container.DataItem) %></td>
                                <td><%#: Item(Container).FavoritedAt.Value.ToString("dd.MM.yyyy") %></td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>
            </table>
        </div>

        <%-- Розгорнута інформація про вибраний рядок. --%>
        <div id="favDetails" class="stub-note" style="display:none;">
            <asp:Repeater ID="rptDetails" runat="server" OnItemCommand="rptDetails_ItemCommand">
                <ItemTemplate>
                    <div class="fav-detail" data-for='<%#: Item(Container).ServiceId %>' style="display:none;">
                        <div style="display:flex;gap:1rem;flex-wrap:wrap;align-items:flex-start;">
                            <asp:Image runat="server" Visible='<%#: Not String.IsNullOrEmpty(Item(Container).ThumbnailUrl) %>'
                                ImageUrl='<%#: If(String.IsNullOrEmpty(Item(Container).ThumbnailUrl), "", ResolveUrl(Item(Container).ThumbnailUrl)) %>'
                                AlternateText="" style="width:200px;max-width:100%;height:150px;object-fit:cover;border-radius:8px;" />
                            <div style="flex:1;min-width:220px;">
                                <h2 style="margin-top:0;"><%#: Item(Container).Title %></h2>
                                <asp:Label runat="server" CssClass="status-badge" Visible='<%# Not IsAvailable(Container.DataItem) %>'
                                    Text="<%$ Resources:SiteText, Favorites_Unavailable %>" />
                                <p class="service-category">
                                    <%#: Item(Container).CategoryName %>
                                    <%#: If(String.IsNullOrEmpty(Item(Container).District), "", " · " & Item(Container).District) %>
                                </p>
                                <p class="catalog-price"><%#: PriceText(Container.DataItem) %> · <%#: RatingText(Container.DataItem) %></p>
                                <p>
                                    <b><asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_Table_Provider %>" />:</b> <%#: Item(Container).ProviderName %>
                                    <asp:PlaceHolder runat="server" Visible='<%# IsAvailable(Container.DataItem) AndAlso Not String.IsNullOrEmpty(Item(Container).Phone) %>'>
                                        <br /><b><asp:Literal runat="server" Text="<%$ Resources:SiteText, Details_PhoneLabel %>" /></b> <%#: Item(Container).Phone %>
                                    </asp:PlaceHolder>
                                </p>
                            </div>
                        </div>
                        <p style="white-space:pre-wrap;"><%#: Item(Container).Description %></p>
                        <div class="service-card-actions">
                            <asp:HyperLink runat="server" CssClass="btn-primary btn-link" Visible='<%# IsAvailable(Container.DataItem) %>'
                                NavigateUrl='<%# "~/ServiceDetails.aspx?id=" & Item(Container).ServiceId %>'
                                Text="<%$ Resources:SiteText, Favorites_OpenListing %>" />
                            <asp:HyperLink runat="server" CssClass="btn-secondary btn-link" Visible='<%# IsAvailable(Container.DataItem) AndAlso IsConsumer %>'
                                NavigateUrl='<%# "~/MessageThread.aspx?serviceId=" & Item(Container).ServiceId & "&consumerId=" & CurrentUserId %>'
                                Text="<%$ Resources:SiteText, Details_MessageLink %>" />
                            <asp:Button runat="server" CommandName="Remove" CausesValidation="false" CssClass="btn-secondary"
                                CommandArgument='<%#: Item(Container).ServiceId %>'
                                Text="<%$ Resources:SiteText, Details_Favorite_Remove %>" />
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>
    </asp:Panel>

    <script>
        (function () {
            var body = document.getElementById('favTableBody');
            if (!body) return;
            var table = document.getElementById('favTable');
            var panel = document.getElementById('favDetails');
            var headers = table.querySelectorAll('th[data-sort]');
            var selected = null;

            function rows() { return Array.prototype.slice.call(body.querySelectorAll('tr')); }

            function select(row) {
                if (!row) return;
                if (selected) { selected.style.background = ''; selected.style.outline = ''; }
                selected = row;
                row.style.background = 'rgba(31, 122, 77, 0.12)';
                row.style.outline = '2px solid rgba(31, 122, 77, 0.5)';
                panel.style.display = '';
                Array.prototype.forEach.call(panel.querySelectorAll('.fav-detail'), function (d) {
                    d.style.display = (d.getAttribute('data-for') === row.dataset.id) ? '' : 'none';
                });
                row.scrollIntoView({ block: 'nearest' });
            }

            // Сортування — той самий принцип, що Catalog.aspx (порожні значення завжди в кінці).
            var sortState = { column: null, dir: 1 };
            function sortBy(column, type) {
                var dir = (sortState.column === column) ? -sortState.dir : 1;
                sortState = { column: column, dir: dir };
                rows().sort(function (a, b) {
                    var av = a.dataset[column] || '', bv = b.dataset[column] || '';
                    if (type === 'number') {
                        var an = av === '' ? null : parseFloat(av), bn = bv === '' ? null : parseFloat(bv);
                        if (an === null && bn === null) return 0;
                        if (an === null) return 1;
                        if (bn === null) return -1;
                        return (an - bn) * dir;
                    }
                    return av.localeCompare(bv, 'uk') * dir;
                }).forEach(function (r) { body.appendChild(r); });
                Array.prototype.forEach.call(headers, function (th) {
                    th.querySelector('.sort-arrow').textContent = (th.dataset.sort === column) ? (dir === 1 ? ' ▲' : ' ▼') : '';
                });
            }
            Array.prototype.forEach.call(headers, function (th) {
                th.addEventListener('click', function () { sortBy(th.dataset.sort, th.dataset.type); });
            });

            body.addEventListener('click', function (e) {
                var row = e.target.closest('tr');
                if (row) { select(row); body.focus({ preventScroll: true }); }
            });
            body.addEventListener('dblclick', function (e) {
                var row = e.target.closest('tr');
                if (row && row.dataset.href) window.location = row.dataset.href;
            });
            body.addEventListener('keydown', function (e) {
                var list = rows(), i = list.indexOf(selected);
                if (e.key === 'ArrowDown') { select(list[Math.min(list.length - 1, i + 1)]); e.preventDefault(); }
                else if (e.key === 'ArrowUp') { select(list[Math.max(0, i - 1)]); e.preventDefault(); }
                else if (e.key === 'Enter' && selected && selected.dataset.href) { window.location = selected.dataset.href; }
            });

            select(rows()[0]);
        })();
    </script>
</asp:Content>
