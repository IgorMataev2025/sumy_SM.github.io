<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="AdminReports.aspx.vb" Inherits="SumyPortal.AdminReports" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    <asp:Literal runat="server" Text="<%$ Resources:SiteText, PageTitle_AdminReports %>" /> — <asp:Literal runat="server" Text="<%$ Resources:SiteText, SiteTitleSuffix %>" />
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Скарги на оголошення</h1>
    <p class="stub-note">
        <a href="~/AdminDashboard.aspx" runat="server">← Адмін-панель</a> ·
        <a href="~/AdminModeration.aspx" runat="server">Модерація</a> ·
        <a href="~/AdminServices.aspx" runat="server">Усі оголошення</a> ·
        <a href="~/AdminCategories.aspx" runat="server">Категорії</a> ·
        <a href="~/AdminUsers.aspx" runat="server">Користувачі</a> ·
        <a href="~/AdminLog.aspx" runat="server">Журнал дій</a> ·
        <a href="~/AdminOnlineUsers.aspx" runat="server">Онлайн</a>
    </p>
    <p class="stub-note">
        Скарга — лише сигнал для адміна, не автоматична модерація: статус
        оголошення сам по собі не змінюється. Щоб зняти з публікації чи
        видалити оголошення — перейдіть на «Усі оголошення».
    </p>

    <asp:Label ID="infoLabel" runat="server" CssClass="stub-note" Visible="false" />

    <asp:Panel ID="emptyPanel" runat="server" Visible="false" CssClass="stub-note">
        Відкритих скарг немає.
    </asp:Panel>

    <asp:Repeater ID="rptReports" runat="server" OnItemCommand="rptReports_ItemCommand">
        <ItemTemplate>
            <div class="service-card">
                <div class="service-card-header">
                    <h3><%#: CType(Container.DataItem, SumyPortal.ServiceReport).ServiceTitle %></h3>
                    <span class="status-badge"><%#: CType(Container.DataItem, SumyPortal.ServiceReport).CreatedAt.ToString("dd.MM.yyyy HH:mm") %></span>
                </div>
                <p class="service-category">
                    Причина: <b><%#: CType(Container.DataItem, SumyPortal.ServiceReport).ReasonLabel %></b> ·
                    Скаржник: <%#: If(String.IsNullOrEmpty(CType(Container.DataItem, SumyPortal.ServiceReport).ReporterEmail), "анонім", CType(Container.DataItem, SumyPortal.ServiceReport).ReporterEmail) %>
                </p>
                <asp:Literal runat="server" Visible='<%#: Not String.IsNullOrEmpty(CType(Container.DataItem, SumyPortal.ServiceReport).Comment) %>'
                    Text='<%#: CType(Container.DataItem, SumyPortal.ServiceReport).Comment %>' />

                <div class="service-card-actions">
                    <a href='<%#: "AdminServiceEdit.aspx?id=" & CType(Container.DataItem, SumyPortal.ServiceReport).ServiceId %>'>Переглянути оголошення</a>
                    <asp:LinkButton runat="server" CommandName="Reviewed"
                        CommandArgument='<%#: CType(Container.DataItem, SumyPortal.ServiceReport).ReportId %>'>
                        ✓ Позначити переглянутою
                    </asp:LinkButton>
                </div>
            </div>
        </ItemTemplate>
    </asp:Repeater>
</asp:Content>
