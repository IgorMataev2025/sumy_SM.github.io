<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="MyServices.aspx.vb" Inherits="SumyPortal.MyServices" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Мої оголошення — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Мої оголошення</h1>

    <p><a href="ServiceEdit.aspx" class="btn-primary btn-link">+ Нове оголошення</a></p>

    <asp:Label ID="infoLabel" runat="server" CssClass="stub-note" Visible="false" />

    <asp:Panel ID="emptyPanel" runat="server" Visible="false" CssClass="stub-note">
        У вас поки немає оголошень.
    </asp:Panel>

    <asp:Repeater ID="rptServices" runat="server" OnItemCommand="rptServices_ItemCommand">
        <ItemTemplate>
            <div class="service-card status-<%#: CType(Container.DataItem, SumyPortal.Service).Status.ToLowerInvariant() %>">
                <div class="service-card-header">
                    <h3><%#: CType(Container.DataItem, SumyPortal.Service).Title %></h3>
                    <span class="status-badge"><%#: CType(Container.DataItem, SumyPortal.Service).StatusLabel %></span>
                </div>
                <p class="service-category"><%#: CType(Container.DataItem, SumyPortal.Service).CategoryName %></p>

                <asp:Literal runat="server" Visible='<%#: Not String.IsNullOrEmpty(CType(Container.DataItem, SumyPortal.Service).RejectReason) %>'
                    Text='<%#: "Причина відхилення: " & CType(Container.DataItem, SumyPortal.Service).RejectReason %>' />

                <div class="service-card-actions">
                    <a href='<%#: "ServiceEdit.aspx?id=" & CType(Container.DataItem, SumyPortal.Service).ServiceId %>'>Редагувати</a>

                    <asp:LinkButton runat="server" CommandName="Submit"
                        CommandArgument='<%#: CType(Container.DataItem, SumyPortal.Service).ServiceId %>'
                        Visible='<%#: CType(Container.DataItem, SumyPortal.Service).Status = "Draft" OrElse CType(Container.DataItem, SumyPortal.Service).Status = "Rejected" %>'>
                        Подати на модерацію
                    </asp:LinkButton>

                    <asp:LinkButton runat="server" CommandName="Unpublish"
                        CommandArgument='<%#: CType(Container.DataItem, SumyPortal.Service).ServiceId %>'
                        Visible='<%#: CType(Container.DataItem, SumyPortal.Service).Status = "Approved" %>'>
                        Зняти з публікації
                    </asp:LinkButton>
                </div>
            </div>
        </ItemTemplate>
    </asp:Repeater>
</asp:Content>
