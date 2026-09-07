Imports System

Namespace SumyPortal

    Public Class AdminCategories
        Inherits AdminPageBase

        ''' <summary>0 — режим додавання нової категорії; інакше — Id категорії, що редагується.</summary>
        Private Property EditCategoryId As Integer
            Get
                Return If(ViewState("EditCategoryId"), 0)
            End Get
            Set(value As Integer)
                ViewState("EditCategoryId") = value
            End Set
        End Property

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not IsPostBack Then
                BindCategories()
            End If
        End Sub

        Private Sub BindCategories()
            rptCategories.DataSource = ServiceCategory.GetAllCategories()
            rptCategories.DataBind()
        End Sub

        Protected Sub rptCategories_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
            Dim categoryId = Convert.ToInt32(e.CommandArgument)

            Select Case e.CommandName
                Case "Edit"
                    Dim category = ServiceCategory.GetAllCategories().Find(Function(c) c.CategoryId = categoryId)
                    If category IsNot Nothing Then
                        EditCategoryId = category.CategoryId
                        txtName.Text = category.Name
                        txtDescription.Text = category.Description
                        formHeading.Text = "Редагування категорії"
                        btnCancel.Visible = True
                    End If

                Case "ToggleActive"
                    Dim category = ServiceCategory.GetAllCategories().Find(Function(c) c.CategoryId = categoryId)
                    If category IsNot Nothing Then
                        ServiceCategory.SetActive(categoryId, Not category.IsActive)
                        ShowInfo(If(category.IsActive, "Категорію деактивовано.", "Категорію активовано."))
                    End If
                    BindCategories()
            End Select
        End Sub

        Protected Sub btnSave_Click(sender As Object, e As EventArgs)
            If Not Page.IsValid Then Return

            Dim name = txtName.Text.Trim()
            Dim description = txtDescription.Text.Trim()

            If EditCategoryId > 0 Then
                ServiceCategory.Update(EditCategoryId, name, description)
                ShowInfo("Категорію оновлено.")
            Else
                ServiceCategory.Create(name, description)
                ShowInfo("Категорію додано.")
            End If

            ResetForm()
            BindCategories()
        End Sub

        Protected Sub btnCancel_Click(sender As Object, e As EventArgs)
            ResetForm()
            BindCategories()
        End Sub

        Private Sub ResetForm()
            EditCategoryId = 0
            txtName.Text = String.Empty
            txtDescription.Text = String.Empty
            formHeading.Text = "Нова категорія"
            btnCancel.Visible = False
        End Sub

        Private Sub ShowInfo(message As String)
            infoLabel.Text = message
            infoLabel.Visible = True
        End Sub

    End Class

End Namespace
