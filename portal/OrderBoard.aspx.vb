Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace SumyPortal

    ''' <summary>"Стіл замовлень" (нова консолідуюча фіча, 2026-09-15, постановка ще
    ''' уточнюватиметься) — USER описує потребу вільним текстом, сторінка показує зведену
    ''' картину ринку (категорія → район → постачальник → кількість оголошень), а не картки
    ''' конкретних оголошень (те вже робить Catalog.aspx). Відкрита анонімним (Web.config).</summary>
    Public Class OrderBoard
        Inherits System.Web.UI.Page

        Protected Overrides Sub InitializeCulture()
            LocalizationHelper.ApplyCulture(Me)
            MyBase.InitializeCulture()
        End Sub

        ''' <summary>Один пункт для рендеру категорії — рядки з Service.OrderBoardRow, уже
        ''' згруповані по CategoryId в BindResults() (LINQ, у пам'яті — сама вибірка з БД
        ''' зазвичай невелика, окремого COUNT-запиту на категорію не потрібно).</summary>
        Public Class CategoryGroup
            Public Property CategoryName As String
            Public Property TotalCount As Integer
            Public Property Rows As List(Of Service.OrderBoardRow)
        End Class

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not IsPostBack Then
                BindResults()
            End If
        End Sub

        Private Sub BindResults()
            Dim keyword = txtQuery.Text.Trim()
            Dim rows = Service.SearchOrderBoard(keyword)

            Dim groups = rows.
                GroupBy(Function(r) New With {r.CategoryId, r.CategoryName}).
                Select(Function(g) New CategoryGroup With {
                    .CategoryName = g.Key.CategoryName,
                    .TotalCount = g.Sum(Function(r) r.ServiceCount),
                    .Rows = g.OrderBy(Function(r) r.District).ThenBy(Function(r) r.ProviderName).ToList()
                }).
                OrderBy(Function(g) g.CategoryName).
                ToList()

            rptCategories.DataSource = groups
            rptCategories.DataBind()

            emptyPanel.Visible = (groups.Count = 0)
            summaryLiteral.Text = If(groups.Count = 0, String.Empty,
                String.Format(Resources.SiteText.OrderBoard_Summary,
                    rows.Sum(Function(r) r.ServiceCount),
                    rows.Select(Function(r) r.ProviderId).Distinct().Count(),
                    groups.Count))
        End Sub

        Protected Sub btnSearch_Click(sender As Object, e As EventArgs)
            BindResults()
        End Sub

        Protected Sub btnReset_Click(sender As Object, e As EventArgs)
            txtQuery.Text = String.Empty
            BindResults()
        End Sub

    End Class

End Namespace
