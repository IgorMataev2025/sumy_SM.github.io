<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="AdminModeration.aspx.vb" Inherits="SumyPortal.AdminModeration" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    <asp:Literal runat="server" Text="<%$ Resources:SiteText, PageTitle_AdminModeration %>" /> — <asp:Literal runat="server" Text="<%$ Resources:SiteText, SiteTitleSuffix %>" />
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Модерація оголошень</h1>
    <p class="stub-note">
        <a href="~/AdminDashboard.aspx" runat="server">← Адмін-панель</a> ·
        <a href="~/AdminServices.aspx" runat="server">Усі оголошення</a> ·
        <a href="~/AdminReports.aspx" runat="server">Скарги</a> ·
        <a href="~/AdminCategories.aspx" runat="server">Категорії</a> ·
        <a href="~/AdminUsers.aspx" runat="server">Користувачі</a> ·
        <a href="~/AdminLog.aspx" runat="server">Журнал дій</a> ·
        <a href="~/AdminOnlineUsers.aspx" runat="server">Онлайн</a>
    </p>

    <asp:Label ID="infoLabel" runat="server" CssClass="stub-note" Visible="false" />

    <asp:Panel ID="emptyPanel" runat="server" Visible="false" CssClass="stub-note">
        Черга модерації порожня.
    </asp:Panel>

    <asp:Repeater ID="rptQueue" runat="server" OnItemCommand="rptQueue_ItemCommand">
        <ItemTemplate>
            <div class="service-card status-pending">
                <div class="service-card-header">
                    <h3><%#: CType(Container.DataItem, SumyPortal.Service).Title %></h3>
                    <span class="status-badge"><%#: CType(Container.DataItem, SumyPortal.Service).CreatedAt.ToString("dd.MM.yyyy HH:mm") %></span>
                </div>
                <p class="service-category">
                    <%#: CType(Container.DataItem, SumyPortal.Service).CategoryName %> ·
                    <%#: CType(Container.DataItem, SumyPortal.Service).ProviderName %>
                    (<%#: CType(Container.DataItem, SumyPortal.Service).ProviderEmail %>)
                </p>
                <p><%#: CType(Container.DataItem, SumyPortal.Service).Description %></p>
                <p>
                    <asp:Literal runat="server" Visible='<%#: CType(Container.DataItem, SumyPortal.Service).Price.HasValue %>'
                        Text='<%#: "Ціна: " & CType(Container.DataItem, SumyPortal.Service).Price & " грн. " %>' />
                    <asp:Literal runat="server" Text='<%#: "Район: " & CType(Container.DataItem, SumyPortal.Service).District & ". Телефон: " & CType(Container.DataItem, SumyPortal.Service).Phone %>' />
                </p>

                <%-- 2026-09-25 (аудит Адміна, п.1): модерація не наосліп — фото, специфікація, мітка
                     на карті й повний вигляд прямо в черзі, без переходу в кожне оголошення. --%>
                <div>
                    <asp:Repeater runat="server" DataSource='<%# PhotosOf(Container.DataItem) %>'>
                        <ItemTemplate>
                            <a class="photo-thumb" href='<%#: ResolveUrl(CType(Container.DataItem, SumyPortal.ServicePhoto).FilePath) %>' target="_blank">
                                <img src='<%#: ResolveUrl(CType(Container.DataItem, SumyPortal.ServicePhoto).FilePath) %>' alt="" />
                            </a>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                <p class="service-category">
                    <asp:Literal runat="server" Visible='<%# PhotosOf(Container.DataItem).Count = 0 %>' Text="Фото немає · " />
                    <asp:HyperLink runat="server" Target="_blank" Text="📄 Специфікація"
                        Visible='<%# Not String.IsNullOrEmpty(CType(Container.DataItem, SumyPortal.Service).SpecificationFilePath) %>'
                        NavigateUrl='<%# If(String.IsNullOrEmpty(CType(Container.DataItem, SumyPortal.Service).SpecificationFilePath), "", ResolveUrl(CType(Container.DataItem, SumyPortal.Service).SpecificationFilePath)) %>' />
                    <asp:HyperLink runat="server" Target="_blank" Text="📍 Мітка на карті"
                        Visible='<%# CType(Container.DataItem, SumyPortal.Service).Latitude.HasValue %>'
                        NavigateUrl='<%# MapUrl(Container.DataItem) %>' />
                    <asp:Literal runat="server" Visible='<%# Not CType(Container.DataItem, SumyPortal.Service).Latitude.HasValue %>' Text="Мітки на карті немає" />
                    · <a href='<%#: ResolveUrl("~/AdminServiceEdit.aspx?id=" & CType(Container.DataItem, SumyPortal.Service).ServiceId) %>'>Відкрити повністю</a>
                </p>

                <div class="form-row">
                    <asp:DropDownList runat="server" ID="ddlRejectTemplate">
                        <asp:ListItem Text="— типова причина відхилення —" Value="" />
                        <asp:ListItem Text="Немає фото послуги" />
                        <asp:ListItem Text="Неповний або незрозумілий опис" />
                        <asp:ListItem Text="Не вказано ціну або умови оплати" />
                        <asp:ListItem Text="Неправильна категорія" />
                        <asp:ListItem Text="Заборонена або незаконна послуга" />
                        <asp:ListItem Text="Дублікат наявного оголошення" />
                        <asp:ListItem Text="Контакти або реклама в тексті/фото" />
                    </asp:DropDownList>
                    <asp:TextBox runat="server" ID="txtRejectReason" placeholder="Своя причина або уточнення" />
                </div>

                <div class="service-card-actions">
                    <asp:LinkButton runat="server" CommandName="Approve"
                        CommandArgument='<%#: CType(Container.DataItem, SumyPortal.Service).ServiceId %>'>
                        ✓ Схвалити
                    </asp:LinkButton>
                    <asp:LinkButton runat="server" CommandName="Reject"
                        CommandArgument='<%#: CType(Container.DataItem, SumyPortal.Service).ServiceId %>'>
                        ✕ Відхилити
                    </asp:LinkButton>
                </div>
            </div>
        </ItemTemplate>
    </asp:Repeater>
</asp:Content>
