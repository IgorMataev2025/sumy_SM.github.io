<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Catalog.aspx.vb" Inherits="SumyPortal.Catalog" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Каталог послуг — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Каталог послуг</h1>

    <asp:Panel ID="filterPanel" runat="server" CssClass="filter-panel">
        <div class="filter-row">
            <div class="form-row">
                <label for="<%= ddlCategory.ClientID %>">Категорія</label>
                <asp:DropDownList ID="ddlCategory" runat="server" DataTextField="Name" DataValueField="CategoryId" />
            </div>
            <div class="form-row">
                <label for="<%= ddlDistrict.ClientID %>">Район</label>
                <asp:DropDownList ID="ddlDistrict" runat="server" />
            </div>
            <div class="form-row">
                <label for="<%= txtKeyword.ClientID %>">Ключове слово</label>
                <asp:TextBox ID="txtKeyword" runat="server" placeholder="напр. ремонт" />
            </div>
        </div>
        <div class="filter-row">
            <div class="form-row">
                <label for="<%= txtMinPrice.ClientID %>">Ціна від</label>
                <asp:TextBox ID="txtMinPrice" runat="server" MaxLength="10" />
                <asp:RegularExpressionValidator runat="server" ControlToValidate="txtMinPrice" ErrorMessage="Число" ValidationExpression="^\d+(\.\d{1,2})?$" Display="Dynamic" CssClass="field-error" />
            </div>
            <div class="form-row">
                <label for="<%= txtMaxPrice.ClientID %>">Ціна до</label>
                <asp:TextBox ID="txtMaxPrice" runat="server" MaxLength="10" />
                <asp:RegularExpressionValidator runat="server" ControlToValidate="txtMaxPrice" ErrorMessage="Число" ValidationExpression="^\d+(\.\d{1,2})?$" Display="Dynamic" CssClass="field-error" />
            </div>
            <div class="form-row filter-actions">
                <asp:Button ID="btnSearch" runat="server" Text="Знайти" OnClick="btnSearch_Click" CssClass="btn-primary" />
                <asp:Button ID="btnReset" runat="server" Text="Скинути" OnClick="btnReset_Click" CausesValidation="false" CssClass="btn-secondary" />
            </div>
        </div>
    </asp:Panel>

    <asp:Label ID="pageInfoLiteral" runat="server" CssClass="page-info" />

    <asp:Panel ID="emptyPanel" runat="server" Visible="false" CssClass="stub-note">
        За заданими критеріями нічого не знайдено.
    </asp:Panel>

    <div class="catalog-grid">
        <asp:Repeater ID="rptCatalog" runat="server">
            <ItemTemplate>
                <a class="catalog-card" href='<%#: "ServiceDetails.aspx?id=" & CType(Container.DataItem, SumyPortal.Service).ServiceId %>'>
                    <div class="catalog-thumb">
                        <asp:Image runat="server" Visible='<%#: Not String.IsNullOrEmpty(CType(Container.DataItem, SumyPortal.Service).ThumbnailUrl) %>'
                            ImageUrl='<%#: If(String.IsNullOrEmpty(CType(Container.DataItem, SumyPortal.Service).ThumbnailUrl), "", ResolveUrl(CType(Container.DataItem, SumyPortal.Service).ThumbnailUrl)) %>' AlternateText="" />
                        <span class="catalog-thumb-placeholder" runat="server" visible='<%#: String.IsNullOrEmpty(CType(Container.DataItem, SumyPortal.Service).ThumbnailUrl) %>'>Фото немає</span>
                    </div>
                    <div class="catalog-card-body">
                        <h3><%#: CType(Container.DataItem, SumyPortal.Service).Title %></h3>
                        <p class="service-category">
                            <%#: CType(Container.DataItem, SumyPortal.Service).CategoryName %>
                            <%#: If(String.IsNullOrEmpty(CType(Container.DataItem, SumyPortal.Service).District), "", " · " & CType(Container.DataItem, SumyPortal.Service).District) %>
                        </p>
                        <p class="catalog-price">
                            <%#: If(CType(Container.DataItem, SumyPortal.Service).Price.HasValue, CType(Container.DataItem, SumyPortal.Service).Price.Value.ToString("0.## грн"), "Ціна за домовленістю") %>
                        </p>
                    </div>
                </a>
            </ItemTemplate>
        </asp:Repeater>
    </div>

    <div class="pagination">
        <asp:LinkButton ID="lnkPrev" runat="server" OnClick="lnkPrev_Click" CausesValidation="false">← Попередня</asp:LinkButton>
        <asp:LinkButton ID="lnkNext" runat="server" OnClick="lnkNext_Click" CausesValidation="false">Наступна →</asp:LinkButton>
    </div>
</asp:Content>
