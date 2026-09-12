<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Profile.aspx.vb" Inherits="SumyPortal.Profile" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Профіль — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Literal ID="headingLiteral" runat="server" /></h1>

    <asp:Panel ID="notFoundPanel" runat="server" Visible="false" CssClass="stub-note">
        <asp:Literal runat="server" Text="<%$ Resources:SiteText, Profile_NotFound %>" />
    </asp:Panel>

    <!-- Публічний перегляд (постановка робочої тестової версії, 2026-09-12,
         Profile.aspx?providerId=X) — лише ім'я/компанія, без форми редагування. -->
    <asp:Panel ID="publicPanel" runat="server" Visible="false" CssClass="stub-note">
        <p><asp:Literal ID="publicNameLiteral" runat="server" /></p>
    </asp:Panel>

    <asp:Label ID="infoLabel" runat="server" CssClass="stub-note" Visible="false" />

    <asp:Panel ID="formPanel" runat="server" CssClass="auth-form">
        <asp:ValidationSummary ID="validationSummary" runat="server" CssClass="form-error" DisplayMode="BulletList" />
        <asp:Label ID="serverErrorLabel" runat="server" CssClass="form-error" Visible="false" />

        <div class="form-row">
            <label>Email</label>
            <asp:Literal ID="emailLiteral" runat="server" />
        </div>

        <div class="form-row">
            <label for="<%= txtFullName.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Register_Label_FullName %>" /></label>
            <asp:TextBox ID="txtFullName" runat="server" MaxLength="255" />
            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtFullName" ErrorMessage="<%$ Resources:SiteText, Register_Val_FullName %>" Display="Dynamic" CssClass="field-error" />
        </div>

        <div class="form-row">
            <label for="<%= txtPhone.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Register_Label_Phone %>" /></label>
            <asp:TextBox ID="txtPhone" runat="server" MaxLength="50" />
        </div>

        <div class="form-row">
            <label for="<%= ddlDistrict.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:SiteText, Register_Label_District %>" /></label>
            <asp:DropDownList ID="ddlDistrict" runat="server" />
        </div>

        <asp:Panel ID="legalEntityPanel" runat="server" Visible="false">
            <div class="form-row">
                <label for="<%= txtCompanyName.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:SiteText, RegisterProvider_Label_CompanyName %>" /></label>
                <asp:TextBox ID="txtCompanyName" runat="server" MaxLength="255" />
            </div>
            <div class="form-row">
                <label for="<%= txtEdrpou.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:SiteText, RegisterProvider_Label_Edrpou %>" /></label>
                <asp:TextBox ID="txtEdrpou" runat="server" MaxLength="20" />
                <asp:RegularExpressionValidator runat="server" ControlToValidate="txtEdrpou" ErrorMessage="<%$ Resources:SiteText, RegisterProvider_Val_Edrpou %>" ValidationExpression="^\d{8}$" Display="Dynamic" CssClass="field-error" />
            </div>
        </asp:Panel>

        <div class="form-row">
            <asp:Button ID="btnSave" runat="server" Text="<%$ Resources:SiteText, Profile_Btn_Save %>" OnClick="btnSave_Click" CssClass="btn-primary" />
        </div>
    </asp:Panel>

    <asp:Panel ID="dangerZonePanel" runat="server" CssClass="stub-note danger-zone">
        <h2><asp:Literal runat="server" Text="<%$ Resources:SiteText, Profile_DangerZone_Heading %>" /></h2>
        <p>
            <asp:Literal runat="server" Text="<%$ Resources:SiteText, Profile_DangerZone_Text %>" />
            <asp:HyperLink runat="server" NavigateUrl="~/PrivacyPolicy.aspx" Target="_blank" Text="<%$ Resources:SiteText, Profile_DangerZone_PrivacyLink %>" />.
        </p>
        <asp:Button ID="btnDeleteAccount" runat="server" Text="<%$ Resources:SiteText, Profile_Btn_DeleteAccount %>" CssClass="btn-secondary"
            OnClick="btnDeleteAccount_Click" CausesValidation="false"
            OnClientClick="<%$ Resources:SiteText, Profile_DeleteConfirmJs %>" />
    </asp:Panel>

    <!-- Галерея (постановка робочої тестової версії, 2026-09-12) — окремо від фото
         оголошень, публічна (бачить будь-хто за Profile.aspx?providerId=X). -->
    <asp:Panel ID="galleryPanel" runat="server" CssClass="stub-note">
        <h2><asp:Literal runat="server" Text="<%$ Resources:SiteText, Profile_Gallery_Heading %>" /></h2>

        <div class="catalog-grid photo-gallery">
            <asp:Repeater ID="rptGallery" runat="server" OnItemCommand="rptGallery_ItemCommand" OnItemDataBound="rptGallery_ItemDataBound">
                <ItemTemplate>
                    <!-- .gallery-tile, а не .photo-thumb (той — для маленьких 100x100 мініатюр
                         ServiceEdit.aspx з чекбоксами; тут .photo-thumb img переважав би
                         .gallery-photo через вищу специфічність CSS-селектора й стискав фото
                         до 100x100 замість повноцінної картки 100%/4:3 — знайдено живим
                         тестом мобільної верстки, 2026-09-12). -->
                    <div class="gallery-tile">
                        <img class="gallery-photo" src='<%#: ResolveUrl(CType(Container.DataItem, SumyPortal.ProviderGalleryPhoto).FilePath) %>' alt="" />
                        <asp:LinkButton ID="lnkDeleteGalleryPhoto" runat="server" CommandName="Delete" CausesValidation="false"
                            CommandArgument='<%#: CType(Container.DataItem, SumyPortal.ProviderGalleryPhoto).PhotoId %>'
                            OnClientClick="<%$ Resources:SiteText, Profile_DeleteGalleryConfirmJs %>"
                            Text="<%$ Resources:SiteText, Profile_Gallery_Delete %>" />
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <asp:Panel ID="noGalleryPhotosPanel" runat="server" Visible="false" CssClass="stub-note">
            <asp:Literal runat="server" Text="<%$ Resources:SiteText, Profile_Gallery_Empty %>" />
        </asp:Panel>

        <asp:Panel ID="galleryUploadPanel" runat="server" Visible="false" CssClass="form-row">
            <label for="<%= galleryFileUpload.ClientID %>">
                <asp:Literal runat="server" Text="<%$ Resources:SiteText, Profile_Gallery_UploadLabel_Prefix %>" />
                <asp:Literal ID="maxGalleryPhotosLiteral" runat="server" />
                <asp:Literal runat="server" Text="<%$ Resources:SiteText, Profile_Gallery_UploadLabel_Suffix %>" />
            </label>
            <asp:FileUpload ID="galleryFileUpload" runat="server" AllowMultiple="true" />
            <asp:Button ID="btnUploadGalleryPhoto" runat="server" Text="<%$ Resources:SiteText, Profile_Gallery_Btn_Upload %>" OnClick="btnUploadGalleryPhoto_Click" CausesValidation="false" CssClass="btn-secondary" />
            <asp:Label ID="galleryErrorLabel" runat="server" CssClass="form-error" Visible="false" />
        </asp:Panel>
    </asp:Panel>
</asp:Content>
