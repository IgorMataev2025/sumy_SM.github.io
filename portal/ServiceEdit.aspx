<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="ServiceEdit.aspx.vb" Inherits="SumyPortal.ServiceEdit" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    <asp:Literal ID="titleLiteral" runat="server" Text="Нове оголошення" /> — Портал послуг Safina
</asp:Content>
<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Leaflet + OpenStreetMap — безкоштовно, без API-ключа (постановка робочої тестової версії, 2026-09-12). -->
    <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"
        integrity="sha256-p4NxAoJBhIIN+hmNHrzRCf9tD/miZyoHS5obTRR9BMY=" crossorigin="" />
    <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"
        integrity="sha256-20nQCchB9co0qIjJZRGuk2/Z9VM+kNiyxNV1lvTlZBo=" crossorigin=""></script>
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Literal ID="headingLiteral" runat="server" Text="Нове оголошення" /></h1>

    <asp:Panel ID="lockedPanel" runat="server" Visible="false" CssClass="stub-note">
        <asp:Literal ID="lockedText" runat="server" />
        <p><a href="MyServices.aspx">← До списку оголошень</a></p>
    </asp:Panel>

    <asp:Panel ID="formPanel" runat="server" CssClass="auth-form service-form">
        <asp:ValidationSummary ID="validationSummary" runat="server" CssClass="form-error" DisplayMode="BulletList" />
        <asp:Label ID="serverErrorLabel" runat="server" CssClass="form-error" Visible="false" />

        <div class="form-row">
            <label for="<%= ddlCategory.ClientID %>">Категорія</label>
            <asp:DropDownList ID="ddlCategory" runat="server" DataTextField="Name" DataValueField="CategoryId" />
        </div>

        <div class="form-row">
            <label for="<%= txtTitle.ClientID %>">Назва</label>
            <asp:TextBox ID="txtTitle" runat="server" MaxLength="255" />
            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtTitle" ErrorMessage="Вкажіть назву" Display="Dynamic" CssClass="field-error" />
        </div>

        <div class="form-row">
            <label for="<%= txtDescription.ClientID %>">Опис</label>
            <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="5" />
        </div>

        <div class="form-row">
            <label for="<%= txtPrice.ClientID %>">Ціна, грн (необов'язково)</label>
            <asp:TextBox ID="txtPrice" runat="server" MaxLength="10" />
            <asp:RegularExpressionValidator runat="server" ControlToValidate="txtPrice" ErrorMessage="Ціна — число, напр. 250 або 250.50" ValidationExpression="^\d+(\.\d{1,2})?$" Display="Dynamic" CssClass="field-error" />
        </div>

        <div class="form-row">
            <label for="<%= ddlDistrict.ClientID %>">Район</label>
            <asp:DropDownList ID="ddlDistrict" runat="server" />
        </div>

        <div class="form-row">
            <label for="<%= txtPhone.ClientID %>">Контактний телефон</label>
            <asp:TextBox ID="txtPhone" runat="server" MaxLength="50" />
            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPhone" ErrorMessage="Вкажіть телефон" Display="Dynamic" CssClass="field-error" />
        </div>

        <div class="form-row">
            <label>Місцезнаходження на карті (необов'язково — клікніть, щоб поставити мітку)</label>
            <div id="serviceMap" style="height:300px; border-radius:8px;"></div>
            <p class="stub-note" id="mapHint">Мітку ще не встановлено.</p>
            <asp:HiddenField ID="hidLatitude" runat="server" />
            <asp:HiddenField ID="hidLongitude" runat="server" />
        </div>

        <asp:Panel ID="existingPhotosPanel" runat="server" Visible="false" CssClass="form-row">
            <label>Наявні фото</label>
            <asp:Repeater ID="rptPhotos" runat="server">
                <ItemTemplate>
                    <div class="photo-thumb">
                        <img src='<%#: ResolveUrl(CType(Container.DataItem, SumyPortal.ServicePhoto).FilePath) %>' alt="" />
                        <label>
                            <asp:CheckBox runat="server" ID="chkDeletePhoto" />
                            видалити
                        </label>
                        <asp:HiddenField runat="server" ID="hidPhotoId" Value='<%#: CType(Container.DataItem, SumyPortal.ServicePhoto).PhotoId %>' />
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </asp:Panel>

        <div class="form-row">
            <label for="<%= fileUpload.ClientID %>">Додати фото (до <asp:Literal ID="maxPhotosLiteral" runat="server" /> шт., jpg/png/gif)</label>
            <asp:FileUpload ID="fileUpload" runat="server" AllowMultiple="true" />
        </div>

        <div class="form-row form-actions">
            <asp:Button ID="btnSaveDraft" runat="server" Text="Зберегти як чернетку" OnClick="btnSaveDraft_Click" CausesValidation="true" CssClass="btn-secondary" />
            <asp:Button ID="btnSubmitModeration" runat="server" Text="Зберегти і подати на модерацію" OnClick="btnSubmitModeration_Click" CausesValidation="true" CssClass="btn-primary" />
        </div>

        <p><a href="MyServices.aspx">← До списку оголошень</a></p>

        <!-- Скрипт усередині formPanel навмисно — Panel Visible="false" (lockedPanel-режим,
             оголошення вже на модерації/опубліковане) не рендерить дітей узагалі, і DOM-елементи
             serviceMap/hidLatitude/hidLongitude тоді відсутні; скрипт зовні панелі впав би на
             getElementById(null). -->
        <script>
            (function () {
                var latField = document.getElementById('<%= hidLatitude.ClientID %>');
                var lngField = document.getElementById('<%= hidLongitude.ClientID %>');
                var hint = document.getElementById('mapHint');
                var hasMarker = !!(latField.value && lngField.value);
                var initialLat = hasMarker ? parseFloat(latField.value) : 50.9077; // центр Сум за замовчуванням
                var initialLng = hasMarker ? parseFloat(lngField.value) : 34.7981;

                var map = L.map('serviceMap').setView([initialLat, initialLng], hasMarker ? 14 : 12);
                L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                    attribution: '&copy; OpenStreetMap contributors',
                    maxZoom: 19
                }).addTo(map);

                var marker = hasMarker ? L.marker([initialLat, initialLng]).addTo(map) : null;
                if (marker) { hint.style.display = 'none'; }

                map.on('click', function (e) {
                    if (marker) {
                        marker.setLatLng(e.latlng);
                    } else {
                        marker = L.marker(e.latlng).addTo(map);
                    }
                    latField.value = e.latlng.lat.toFixed(6);
                    lngField.value = e.latlng.lng.toFixed(6);
                    hint.style.display = 'none';
                });
            })();
        </script>
    </asp:Panel>
</asp:Content>
