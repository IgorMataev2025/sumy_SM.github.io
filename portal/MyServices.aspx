<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="MyServices.aspx.vb" Inherits="SumyPortal.MyServices" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Мої оголошення — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Literal runat="server" Text="<%$ Resources:SiteText, MyServices_Heading %>" /></h1>

    <p><a href="ServiceEdit.aspx" class="btn-primary btn-link"><asp:Literal runat="server" Text="<%$ Resources:SiteText, MyServices_NewLink %>" /></a></p>

    <asp:Label ID="infoLabel" runat="server" CssClass="stub-note" Visible="false" />

    <asp:Panel ID="emptyPanel" runat="server" Visible="false" CssClass="stub-note">
        <asp:Literal runat="server" Text="<%$ Resources:SiteText, MyServices_Empty %>" />
    </asp:Panel>

    <asp:Repeater ID="rptServices" runat="server" OnItemCommand="rptServices_ItemCommand" OnItemDataBound="rptServices_ItemDataBound">
        <ItemTemplate>
            <div class="service-card status-<%#: CType(Container.DataItem, SumyPortal.Service).Status.ToLowerInvariant() %>">
                <div class="service-card-header">
                    <h3><%#: CType(Container.DataItem, SumyPortal.Service).Title %></h3>
                    <span class="status-badge"><%#: CType(Container.DataItem, SumyPortal.Service).StatusLabel %></span>
                </div>
                <p class="service-category"><%#: CType(Container.DataItem, SumyPortal.Service).CategoryName %></p>

                <!-- Статистика для постачальника (п.17, наступна фіча понад MVP, 2026-09-12) —
                     текст формується в rptServices_ItemDataBound (MyServices.aspx.vb), а не тут,
                     бо потребує форматування decimal/умовного "немає відгуків". -->
                <p class="service-category"><asp:Literal ID="statsLiteral" runat="server" /></p>

                <asp:Literal runat="server" Visible='<%#: Not String.IsNullOrEmpty(CType(Container.DataItem, SumyPortal.Service).RejectReason) %>'
                    Text='<%#: Resources.SiteText.MyServices_RejectReasonPrefix & CType(Container.DataItem, SumyPortal.Service).RejectReason %>' />

                <div class="service-card-actions">
                    <a href='<%#: "ServiceEdit.aspx?id=" & CType(Container.DataItem, SumyPortal.Service).ServiceId %>'><%#: Resources.SiteText.MyServices_Edit %></a>

                    <asp:LinkButton runat="server" CommandName="Submit"
                        CommandArgument='<%#: CType(Container.DataItem, SumyPortal.Service).ServiceId %>'
                        Visible='<%#: CType(Container.DataItem, SumyPortal.Service).Status = "Draft" OrElse CType(Container.DataItem, SumyPortal.Service).Status = "Rejected" %>'
                        Text="<%$ Resources:SiteText, MyServices_Submit %>" />

                    <asp:LinkButton runat="server" CommandName="Unpublish"
                        CommandArgument='<%#: CType(Container.DataItem, SumyPortal.Service).ServiceId %>'
                        Visible='<%#: CType(Container.DataItem, SumyPortal.Service).Status = "Approved" %>'
                        Text="<%$ Resources:SiteText, MyServices_Unpublish %>" />
                </div>
            </div>
        </ItemTemplate>
    </asp:Repeater>
</asp:Content>
