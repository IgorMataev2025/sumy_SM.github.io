Imports System
Imports System.Globalization

Namespace SumyPortal

    ''' <summary>
    ''' Динамічна генерація договору про надання послуг для конкретного оголошення
    ''' (уточнена постановка від 2026-09-07, п.3: "законні методи взаємовідносин
    ''' Постачальник ↔ Споживач"). Шаблон спирається на загальні положення глави 63
    ''' ЦК України (ст. 901–907) та Закон "Про захист прав споживачів" — це типовий
    ''' каркас, а не індивідуальна юридична консультація (див. застереження на сторінці).
    ''' Доступна будь-якому автентифікованому користувачу порталу (Consumer чи
    ''' Provider — обидва можуть бути замовниками чужої послуги), крім власника
    ''' самого оголошення.
    ''' </summary>
    Public Class ServiceContract
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

            backLink.NavigateUrl = ResolveUrl("~/ServiceDetails.aspx?id=" & serviceId)

            Dim currentUser = UserAccount.FindByEmail(Page.User.Identity.Name)
            If currentUser Is Nothing Then
                ShowNotFound()
                Return
            End If

            If currentUser.UserId = svc.ProviderId Then
                contractPanel.Visible = False
                ownerPanel.Visible = True
                Return
            End If

            Dim provider = UserAccount.GetById(svc.ProviderId)
            If provider Is Nothing Then
                ShowNotFound()
                Return
            End If

            Dim ukCulture = New CultureInfo("uk-UA")
            contractDateLiteral.Text = Server.HtmlEncode(DateTime.Now.ToString("d MMMM yyyy", ukCulture) & " р.")
            contractNumberLiteral.Text = Server.HtmlEncode("S" & svc.ServiceId & "-U" & currentUser.UserId)

            executorBlockLiteral.Text = FormatPartyIntro(provider)
            customerBlockLiteral.Text = FormatPartyIntro(currentUser)
            executorDetailsLiteral.Text = FormatPartyDetails(provider)
            customerDetailsLiteral.Text = FormatPartyDetails(currentUser)

            serviceTitleLiteral.Text = Server.HtmlEncode(svc.Title)
            categoryLiteral.Text = Server.HtmlEncode(svc.CategoryName)
            descriptionLiteral.Text = Server.HtmlEncode(If(svc.Description, "не вказано"))
            districtLiteral.Text = Server.HtmlEncode(If(String.IsNullOrEmpty(svc.District), "не вказано", svc.District))
            priceLiteral.Text = Server.HtmlEncode(If(svc.Price.HasValue, svc.Price.Value.ToString("0.## грн"), "за домовленістю Сторін"))
        End Sub

        ''' <summary>Короткий вступний опис сторони договору — юрособа "в особі ..." або фізична особа.</summary>
        Private Function FormatPartyIntro(person As UserAccount) As String
            If person.IsLegalEntity Then
                Return Server.HtmlEncode(String.Format("{0} (код ЄДРПОУ {1}), в особі {2}",
                    person.CompanyName, person.EDRPOU, person.FullName))
            End If
            Return Server.HtmlEncode(person.FullName & ", фізична особа")
        End Function

        ''' <summary>Розгорнутий блок реквізитів сторони для таблиці підписів.</summary>
        Private Function FormatPartyDetails(person As UserAccount) As String
            Dim sb As New Text.StringBuilder()
            If person.IsLegalEntity Then
                sb.AppendFormat("{0}<br/>Код ЄДРПОУ: {1}<br/>", Server.HtmlEncode(person.CompanyName), Server.HtmlEncode(person.EDRPOU))
            End If
            sb.AppendFormat("ПІБ: {0}<br/>", Server.HtmlEncode(person.FullName))
            sb.AppendFormat("Телефон: {0}<br/>", Server.HtmlEncode(If(String.IsNullOrEmpty(person.Phone), "не вказано", person.Phone)))
            sb.AppendFormat("Email: {0}", Server.HtmlEncode(person.Email))
            Return sb.ToString()
        End Function

        Private Sub ShowNotFound()
            contractPanel.Visible = False
            ownerPanel.Visible = False
            notFoundPanel.Visible = True
        End Sub

    End Class

End Namespace
