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
        Скарги згруповано за оголошенням. «Зняти оголошення» переводить його у «Відхилено» з
        вказаною причиною (постачальник отримає лист і зможе виправити й подати знову) і
        закриває всі скарги на нього. «Скарги безпідставні» — лише закриває скарги.
    </p>

    <asp:Label ID="infoLabel" runat="server" CssClass="stub-note" Visible="false" />

    <asp:Panel ID="emptyPanel" runat="server" Visible="false" CssClass="stub-note">
        Відкритих скарг немає.
    </asp:Panel>

    <%-- 2026-09-25 (аудит Адміна, п.3): групування за оголошенням + дія прямо зі скарги. --%>
    <asp:Repeater ID="rptGroups" runat="server" OnItemCommand="rptGroups_ItemCommand">
        <ItemTemplate>
            <div class="service-card">
                <div class="service-card-header">
                    <h3><%#: CType(Container.DataItem, SumyPortal.ServiceReportGroup).ServiceTitle %></h3>
                    <span class="status-badge">
                        Скарг: <b><%#: CType(Container.DataItem, SumyPortal.ServiceReportGroup).Reports.Count %></b> ·
                        <%#: New SumyPortal.Service With {.Status = CType(Container.DataItem, SumyPortal.ServiceReportGroup).ServiceStatus}.StatusLabel %>
                    </span>
                </div>
                <asp:Repeater runat="server" DataSource='<%# CType(Container.DataItem, SumyPortal.ServiceReportGroup).Reports %>'>
                    <ItemTemplate>
                        <p class="service-category">
                            <%#: CType(Container.DataItem, SumyPortal.ServiceReport).CreatedAt.ToString("dd.MM.yyyy HH:mm") %> ·
                            <b><%#: CType(Container.DataItem, SumyPortal.ServiceReport).ReasonLabel %></b> ·
                            <%#: If(String.IsNullOrEmpty(CType(Container.DataItem, SumyPortal.ServiceReport).ReporterEmail), "анонім", CType(Container.DataItem, SumyPortal.ServiceReport).ReporterEmail) %>
                            <%#: If(String.IsNullOrEmpty(CType(Container.DataItem, SumyPortal.ServiceReport).Comment), "", " — " & CType(Container.DataItem, SumyPortal.ServiceReport).Comment) %>
                        </p>
                    </ItemTemplate>
                </asp:Repeater>

                <asp:Panel runat="server" CssClass="form-row" Visible='<%# CType(Container.DataItem, SumyPortal.ServiceReportGroup).IsPublished %>'>
                    <asp:TextBox runat="server" ID="txtTakeDownReason" placeholder="Причина для постачальника (порожньо — причини зі скарг)" />
                </asp:Panel>

                <div class="service-card-actions">
                    <a href='<%#: "ServiceDetails.aspx?id=" & CType(Container.DataItem, SumyPortal.ServiceReportGroup).ServiceId %>' target="_blank">Як бачать відвідувачі</a>
                    <a href='<%#: "AdminServiceEdit.aspx?id=" & CType(Container.DataItem, SumyPortal.ServiceReportGroup).ServiceId %>'>Редагувати</a>
                    <asp:LinkButton runat="server" CommandName="TakeDown" Visible='<%# CType(Container.DataItem, SumyPortal.ServiceReportGroup).IsPublished %>'
                        CommandArgument='<%#: CType(Container.DataItem, SumyPortal.ServiceReportGroup).ServiceId %>'
                        OnClientClick="return confirm('Зняти оголошення з публікації й закрити скарги?');">
                        ✕ Зняти оголошення
                    </asp:LinkButton>
                    <asp:LinkButton runat="server" CommandName="Dismiss"
                        CommandArgument='<%#: CType(Container.DataItem, SumyPortal.ServiceReportGroup).ServiceId %>'>
                        ✓ Скарги безпідставні
                    </asp:LinkButton>
                </div>
            </div>
        </ItemTemplate>
    </asp:Repeater>
</asp:Content>
