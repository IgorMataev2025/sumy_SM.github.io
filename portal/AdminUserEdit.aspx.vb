Imports System
Imports MySql.Data.MySqlClient

Namespace SumyPortal

    ''' <summary>
    ''' Редагування даних користувача адміністратором (за прямим запитом
    ''' користувача 2026-09-16, поруч із "Заблокувати" на AdminUsers.aspx).
    ''' Ті самі поля, що Profile.aspx (самостійне редагування) — Email і тип
    ''' акаунта (UserType/IsLegalEntity) тут теж не змінюються (п.12 уточненої
    ''' постановки: тип фіксується при реєстрації). Блокування/розблокування —
    ''' і далі окремою дією на AdminUsers.aspx, тут не дублюється.
    ''' </summary>
    Public Class AdminUserEdit
        Inherits AdminPageBase

        Private Target As UserAccount

        Private ReadOnly Property UserIdParam As Integer
            Get
                Dim id As Integer
                Integer.TryParse(Request.QueryString("id"), id)
                Return id
            End Get
        End Property

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            Target = UserAccount.GetById(UserIdParam)
            If Target Is Nothing Then
                formPanel.Visible = False
                notFoundPanel.Visible = True
                Return
            End If

            emailLiteral.Text = Server.HtmlEncode(Target.Email)

            If Not IsPostBack Then
                SumyDistricts.Populate(ddlDistrict, "Не вказано")
                BindForm()
            End If
        End Sub

        Private Sub BindForm()
            txtFullName.Text = Target.FullName
            txtPhone.Text = Target.Phone

            If ddlDistrict.Items.FindByValue(Target.District) IsNot Nothing Then
                ddlDistrict.SelectedValue = Target.District
            End If

            legalEntityPanel.Visible = Target.IsLegalEntity
            If Target.IsLegalEntity Then
                txtCompanyName.Text = Target.CompanyName
                txtEdrpou.Text = Target.EDRPOU
            End If
        End Sub

        Protected Sub btnSave_Click(sender As Object, e As EventArgs)
            If Target Is Nothing Then Return
            If Not Page.IsValid Then Return

            Dim companyName = If(Target.IsLegalEntity, txtCompanyName.Text.Trim(), Nothing)
            Dim edrpou = If(Target.IsLegalEntity, txtEdrpou.Text.Trim(), Nothing)

            If Target.IsLegalEntity AndAlso String.IsNullOrWhiteSpace(companyName) Then
                ShowError("Вкажіть назву компанії.")
                Return
            End If
            If Target.IsLegalEntity AndAlso String.IsNullOrWhiteSpace(edrpou) Then
                ShowError("Вкажіть ЄДРПОУ.")
                Return
            End If

            Try
                UserAccount.UpdateProfile(Target.UserId, txtFullName.Text.Trim(), txtPhone.Text.Trim(),
                                           ddlDistrict.SelectedValue, companyName, edrpou)
                AdminActionLog.Log(CurrentAdmin.UserId, "Редагував користувача", Target.Email)

                Target = UserAccount.GetById(Target.UserId)
                BindForm()
                infoLabel.Text = "Збережено."
                infoLabel.Visible = True
            Catch ex As MySqlException
                ShowError("Не вдалося зберегти зміни.")
            End Try
        End Sub

        Private Sub ShowError(message As String)
            serverErrorLabel.Text = message
            serverErrorLabel.Visible = True
        End Sub

    End Class

End Namespace
