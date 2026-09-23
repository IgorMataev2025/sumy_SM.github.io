<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="OrderBoard.aspx.vb" Inherits="SumyPortal.OrderBoard" %>
<%@ MasterType VirtualPath="~/Site.master" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    <asp:Literal runat="server" Text="<%$ Resources:SiteText, OrderBoard_Heading %>" /> — <asp:Literal runat="server" Text="<%$ Resources:SiteText, SiteTitleSuffix %>" />
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Literal runat="server" Text="<%$ Resources:SiteText, OrderBoard_Heading %>" /></h1>
    <p><asp:Literal runat="server" Text="<%$ Resources:SiteText, OrderBoard_Intro %>" /></p>

    <asp:Panel ID="filterPanel" runat="server" CssClass="filter-panel" DefaultButton="btnSearch">
        <div class="filter-row">
            <div class="form-row">
                <label for="<%= txtQuery.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:SiteText, OrderBoard_Label_Query %>" /></label>
                <asp:TextBox ID="txtQuery" runat="server" placeholder="<%$ Resources:SiteText, OrderBoard_Placeholder %>" />
            </div>
            <div class="form-row filter-actions">
                <asp:Button ID="btnSearch" runat="server" Text="<%$ Resources:SiteText, OrderBoard_Btn_Search %>" OnClick="btnSearch_Click" CssClass="btn-primary" />
                <asp:Button ID="btnReset" runat="server" Text="<%$ Resources:SiteText, OrderBoard_Btn_Reset %>" OnClick="btnReset_Click" CausesValidation="false" CssClass="btn-secondary" />
            </div>
        </div>
    </asp:Panel>

    <asp:Label ID="summaryLiteral" runat="server" CssClass="page-info" />

    <asp:Panel ID="emptyPanel" runat="server" Visible="false" CssClass="stub-note">
        <asp:Literal runat="server" Text="<%$ Resources:SiteText, OrderBoard_Empty %>" />
    </asp:Panel>

    <asp:Repeater ID="rptCategories" runat="server">
        <ItemTemplate>
            <section class="order-board-category">
                <h2>
                    <%#: CType(Container.DataItem, SumyPortal.OrderBoard.CategoryGroup).CategoryName %>
                    <span class="page-info">(<%#: CType(Container.DataItem, SumyPortal.OrderBoard.CategoryGroup).TotalCount %>)</span>
                </h2>
                <div class="catalog-table-wrap">
                    <table class="catalog-table">
                        <thead>
                            <tr>
                                <th><asp:Literal runat="server" Text="<%$ Resources:SiteText, Catalog_Label_District %>" /></th>
                                <th><asp:Literal runat="server" Text="<%$ Resources:SiteText, OrderBoard_Column_Provider %>" /></th>
                                <th><asp:Literal runat="server" Text="<%$ Resources:SiteText, OrderBoard_Column_Count %>" /></th>
                                <th></th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptRows" runat="server" DataSource='<%# CType(Container.DataItem, SumyPortal.OrderBoard.CategoryGroup).Rows %>'>
                                <ItemTemplate>
                                    <tr>
                                        <td><%#: If(String.IsNullOrEmpty(CType(Container.DataItem, SumyPortal.Service.OrderBoardRow).District), Resources.SiteText.Details_NotSpecified, CType(Container.DataItem, SumyPortal.Service.OrderBoardRow).District) %></td>
                                        <td><%#: CType(Container.DataItem, SumyPortal.Service.OrderBoardRow).ProviderName %></td>
                                        <td><%#: CType(Container.DataItem, SumyPortal.Service.OrderBoardRow).ServiceCount %></td>
                                        <td>
                                            <a href='<%#: "Profile.aspx?providerId=" & CType(Container.DataItem, SumyPortal.Service.OrderBoardRow).ProviderId %>'>
                                                <asp:Literal runat="server" Text="<%$ Resources:SiteText, OrderBoard_Column_Profile %>" />
                                            </a>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </div>
            </section>
        </ItemTemplate>
    </asp:Repeater>
</asp:Content>
