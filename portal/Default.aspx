<%@ Page Title="" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Default.aspx.vb" Inherits="SumyPortal.Default" %>
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Головна — Портал послуг Safina
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <section class="hero">
        <h1>Портал послуг міста та області Суми</h1>
        <p>
            Постачальники розміщують інформацію про свої послуги, а мешканці —
            знаходять і користуються нею. Перегляд оголошень доступний після реєстрації.
        </p>
    </section>

    <section class="categories">
        <h2>Категорії послуг</h2>
        <asp:Panel ID="dbErrorPanel" runat="server" Visible="false" CssClass="stub-note">
            Тимчасово немає з'єднання з базою даних. Спробуйте оновити сторінку пізніше.
        </asp:Panel>
        <asp:Repeater ID="rptCategories" runat="server">
            <ItemTemplate>
                <a class="category-card" href='<%#: "Catalog.aspx?categoryId=" & CType(Container.DataItem, SumyPortal.ServiceCategory).CategoryId %>'>
                    <h3><%#: CType(Container.DataItem, SumyPortal.ServiceCategory).Name %></h3>
                    <p><%#: CType(Container.DataItem, SumyPortal.ServiceCategory).Description %></p>
                </a>
            </ItemTemplate>
        </asp:Repeater>
    </section>
</asp:Content>
