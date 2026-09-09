<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="AdminModeration.aspx.vb" Inherits="SumyPortal.AdminModeration" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Модерація оголошень — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Модерація оголошень</h1>
    <p class="stub-note">
        <a href="~/AdminDashboard.aspx" runat="server">← Адмін-панель</a> ·
        <a href="~/AdminCategories.aspx" runat="server">Категорії</a> ·
        <a href="~/AdminUsers.aspx" runat="server">Користувачі</a> ·
        <a href="~/AdminLog.aspx" runat="server">Журнал дій</a>
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

                <div class="form-row">
                    <asp:TextBox runat="server" ID="txtRejectReason" placeholder="Причина відхилення (обов'язково для відмови)" />
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
