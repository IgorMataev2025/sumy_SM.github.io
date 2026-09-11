<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="AdminServiceEdit.aspx.vb" Inherits="SumyPortal.AdminServiceEdit" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Редагування оголошення (адмін) — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Редагування оголошення</h1>
    <p class="stub-note">
        <a href="~/AdminServices.aspx" runat="server">← Усі оголошення</a> ·
        <asp:Literal ID="providerLiteral" runat="server" />
    </p>

    <asp:Panel ID="notFoundPanel" runat="server" Visible="false" CssClass="stub-note">
        Оголошення не знайдено.
    </asp:Panel>

    <asp:Panel ID="formPanel" runat="server" CssClass="auth-form service-form">
        <asp:ValidationSummary ID="validationSummary" runat="server" CssClass="form-error" DisplayMode="BulletList" />
        <asp:Label ID="serverErrorLabel" runat="server" CssClass="form-error" Visible="false" />
        <asp:Label ID="infoLabel" runat="server" CssClass="stub-note" Visible="false" />

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
            <label for="<%= ddlStatus.ClientID %>">Статус</label>
            <asp:DropDownList ID="ddlStatus" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlStatus_SelectedIndexChanged">
                <asp:ListItem Text="Чернетка" Value="Draft" />
                <asp:ListItem Text="На модерації" Value="Pending" />
                <asp:ListItem Text="Опубліковано" Value="Approved" />
                <asp:ListItem Text="Відхилено" Value="Rejected" />
            </asp:DropDownList>
        </div>

        <asp:Panel ID="rejectReasonPanel" runat="server" CssClass="form-row" Visible="false">
            <label for="<%= txtRejectReason.ClientID %>">Причина відхилення (видима постачальнику)</label>
            <asp:TextBox ID="txtRejectReason" runat="server" MaxLength="500" />
        </asp:Panel>

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
            <asp:Button ID="btnSave" runat="server" Text="Зберегти" OnClick="btnSave_Click" CausesValidation="true" CssClass="btn-primary" />
        </div>
    </asp:Panel>

    <asp:Panel ID="reviewsPanel" runat="server" Visible="false" CssClass="reviews-section">
        <h2>Відгуки на це оголошення</h2>
        <p class="stub-note">Адмін може лише видаляти відгуки (модерація образливого/спам-контенту) — редагувати чужий текст не можна.</p>
        <asp:Repeater ID="rptReviews" runat="server" OnItemCommand="rptReviews_ItemCommand">
            <ItemTemplate>
                <div class="review-card">
                    <p class="review-meta">
                        <b><%#: CType(Container.DataItem, SumyPortal.Review).ConsumerName %></b> ·
                        <%#: New String("★"c, CType(Container.DataItem, SumyPortal.Review).Rating) & New String("☆"c, 5 - CType(Container.DataItem, SumyPortal.Review).Rating) %> ·
                        <%#: CType(Container.DataItem, SumyPortal.Review).CreatedAt.ToString("dd.MM.yyyy") %>
                    </p>
                    <asp:Literal runat="server" Visible='<%#: Not String.IsNullOrEmpty(CType(Container.DataItem, SumyPortal.Review).Comment) %>'
                        Text='<%#: CType(Container.DataItem, SumyPortal.Review).Comment %>' />
                    <p>
                        <asp:LinkButton runat="server" CommandName="Delete"
                            CommandArgument='<%#: CType(Container.DataItem, SumyPortal.Review).ReviewId %>'
                            OnClientClick="return confirm('Видалити цей відгук назавжди?');">
                            Видалити відгук
                        </asp:LinkButton>
                    </p>
                </div>
            </ItemTemplate>
        </asp:Repeater>
        <asp:Panel ID="noReviewsPanel" runat="server" Visible="false" CssClass="stub-note">
            Відгуків ще немає.
        </asp:Panel>
    </asp:Panel>
</asp:Content>
