Imports System
Imports System.Web.Security
Imports MySql.Data.MySqlClient

Namespace SumyPortal

    ''' <summary>
    ''' Особистий кабінет (п.6 уточненої постановки) — редагування власних даних
    ''' і самостійне видалення акаунта. Доступний будь-якому залогіненому
    ''' користувачу (Consumer і Provider), захищений стандартним
    ''' Web.config: deny users="?" — окремого <location> не потрібно
    ''' (той самий підхід, що MyServices.aspx).
    ''' </summary>
    Public Class Profile
        Inherits System.Web.UI.Page

        Private CurrentUser As UserAccount

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            CurrentUser = UserAccount.FindByEmail(Page.User.Identity.Name)
            If CurrentUser Is Nothing Then
                Response.Redirect("~/Default.aspx", True)
                Return
            End If

            If Not IsPostBack Then
                SumyDistricts.Populate(ddlDistrict, "Не вказано")
                BindProfile()
            End If
        End Sub

        Private Sub BindProfile()
            emailLiteral.Text = Server.HtmlEncode(CurrentUser.Email)
            txtFullName.Text = CurrentUser.FullName
            txtPhone.Text = CurrentUser.Phone

            If ddlDistrict.Items.FindByValue(CurrentUser.District) IsNot Nothing Then
                ddlDistrict.SelectedValue = CurrentUser.District
            End If

            legalEntityPanel.Visible = CurrentUser.IsLegalEntity
            If CurrentUser.IsLegalEntity Then
                txtCompanyName.Text = CurrentUser.CompanyName
                txtEdrpou.Text = CurrentUser.EDRPOU
            End If

            ' Самовидалення адміна не передбачено (UserAccount.DeleteAccount це й
            ' блокує на рівні SQL) — ховаємо кнопку, а не показуємо помилку постфактум.
            dangerZonePanel.Visible = Not CurrentUser.IsAdmin
        End Sub

        Protected Sub btnSave_Click(sender As Object, e As EventArgs)
            If Not Page.IsValid Then Return

            Dim companyName = If(CurrentUser.IsLegalEntity, txtCompanyName.Text.Trim(), Nothing)
            Dim edrpou = If(CurrentUser.IsLegalEntity, txtEdrpou.Text.Trim(), Nothing)

            If CurrentUser.IsLegalEntity AndAlso String.IsNullOrWhiteSpace(companyName) Then
                ShowError("Вкажіть назву компанії.")
                Return
            End If
            If CurrentUser.IsLegalEntity AndAlso String.IsNullOrWhiteSpace(edrpou) Then
                ShowError("Вкажіть ЄДРПОУ.")
                Return
            End If

            Try
                UserAccount.UpdateProfile(CurrentUser.UserId, txtFullName.Text.Trim(), txtPhone.Text.Trim(),
                                           ddlDistrict.SelectedValue, companyName, edrpou)
                infoLabel.Text = "Дані збережено."
                infoLabel.Visible = True
                CurrentUser = UserAccount.GetById(CurrentUser.UserId)
                BindProfile()
            Catch ex As MySqlException
                ShowError("Не вдалося зберегти — спробуйте пізніше.")
            End Try
        End Sub

        Protected Sub btnDeleteAccount_Click(sender As Object, e As EventArgs)
            If UserAccount.DeleteAccount(CurrentUser.UserId) Then
                FormsAuthentication.SignOut()
                Response.Redirect("~/Default.aspx", True)
            Else
                ShowError("Не вдалося видалити акаунт — спробуйте пізніше.")
            End If
        End Sub

        Private Sub ShowError(message As String)
            serverErrorLabel.Text = message
            serverErrorLabel.Visible = True
        End Sub

    End Class

End Namespace
