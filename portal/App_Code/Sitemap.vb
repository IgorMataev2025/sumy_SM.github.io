Imports System
Imports System.Text
Imports System.Web

Namespace SumyPortal

    ''' <summary>
    ''' Динамічний sitemap.xml (п.21, наступна фіча понад MVP — базове SEO,
    ''' 2026-09-12) — той самий прийом IHttpHandler в App_Code, що вже
    ''' SetLanguage.ashx (клас не в окремому CodeBehind — стандартно для .ashx
    ''' у файловій моделі Website Project). Домен береться із самого запиту
    ''' (Request.Url), а не хардкодиться — коректний і зараз (*.etempurl.com),
    ''' і після купівлі власного домену без змін коду (той самий підхід, що вже
    ''' Donate.aspx.vb для resultUrl/serverUrl LiqPay).
    ''' </summary>
    Public Class Sitemap
        Implements IHttpHandler

        Public ReadOnly Property IsReusable As Boolean Implements IHttpHandler.IsReusable
            Get
                Return True
            End Get
        End Property

        Public Sub ProcessRequest(context As HttpContext) Implements IHttpHandler.ProcessRequest
            Dim baseUrl = context.Request.Url.GetLeftPart(UriPartial.Authority)

            Dim sb As New StringBuilder()
            sb.Append("<?xml version=""1.0"" encoding=""UTF-8""?>")
            sb.Append("<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">")

            AppendUrl(sb, baseUrl & "/Default.aspx")
            AppendUrl(sb, baseUrl & "/Catalog.aspx")
            AppendUrl(sb, baseUrl & "/LegalGuide.aspx")

            ' Лише опубліковані оголошення — той самий фільтр, що каталог/картка.
            For Each item In Service.GetApprovedForSitemap()
                AppendUrl(sb, baseUrl & "/ServiceDetails.aspx?id=" & item.Key, item.Value)
            Next

            sb.Append("</urlset>")

            context.Response.ContentType = "application/xml"
            context.Response.ContentEncoding = System.Text.Encoding.UTF8
            context.Response.Write(sb.ToString())
        End Sub

        ''' <summary>HtmlEncode тут не про XSS (немає браузера-жертви), а про те, що XML
        ''' і HTML ділять той самий базовий набір екранованих символів (&amp;/&lt;/&gt;/&quot;) —
        ''' той самий прийом economно повторно використано, а не власний XML-екранер.</summary>
        Private Sub AppendUrl(sb As StringBuilder, url As String, Optional lastMod As DateTime? = Nothing)
            sb.Append("<url><loc>").Append(HttpUtility.HtmlEncode(url)).Append("</loc>")
            If lastMod.HasValue Then
                sb.Append("<lastmod>").Append(lastMod.Value.ToString("yyyy-MM-dd")).Append("</lastmod>")
            End If
            sb.Append("</url>")
        End Sub

    End Class

End Namespace
