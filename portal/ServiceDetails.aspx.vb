Imports System

Namespace SumyPortal

    Public Class ServiceDetails
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If IsPostBack Then Return

            Dim serviceId As Integer
            If Not Integer.TryParse(Request.QueryString("id"), serviceId) Then
                ShowNotFound()
                Return
            End If

            Dim svc = Service.GetApprovedById(serviceId)
            If svc Is Nothing Then
                ShowNotFound()
                Return
            End If

            titleLiteral.Text = svc.Title
            headingLiteral.Text = svc.Title
            categoryLiteral.Text = svc.CategoryName
            priceLiteral.Text = If(svc.Price.HasValue, svc.Price.Value.ToString("0.## грн"), "Ціна за домовленістю")
            descriptionLiteral.Text = svc.Description
            providerNameLiteral.Text = svc.ProviderName
            phoneLiteral.Text = svc.Phone
            districtLiteral.Text = If(String.IsNullOrEmpty(svc.District), "не вказано", svc.District)

            Dim photos = Service.GetPhotos(svc.ServiceId)
            rptPhotos.DataSource = photos
            rptPhotos.DataBind()

            ' Договір формується для замовника, а не для власника оголошення (уточнена
            ' постановка від 2026-09-07, п.3) — посилання ховаємо, якщо дивиться сам постачальник.
            Dim currentUser = UserAccount.FindByEmail(Page.User.Identity.Name)
            contractLink.Visible = (currentUser IsNot Nothing AndAlso currentUser.UserId <> svc.ProviderId)
            contractLink.NavigateUrl = ResolveUrl("~/ServiceContract.aspx?id=" & svc.ServiceId)
        End Sub

        Private Sub ShowNotFound()
            detailsPanel.Visible = False
            notFoundPanel.Visible = True
        End Sub

    End Class

End Namespace
