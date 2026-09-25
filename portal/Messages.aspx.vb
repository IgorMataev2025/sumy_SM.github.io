Imports System

Namespace SumyPortal

    ''' <summary>Список розмов поточного користувача (п.10 уточненої постановки) — своя
    ''' вибірка для Consumer і для Provider (один може писати з кількох оголошень,
    ''' другому може писати кілька різних споживачів по одному оголошенню).</summary>
    Public Class Messages
        Inherits System.Web.UI.Page

        Protected Overrides Sub InitializeCulture()
            LocalizationHelper.ApplyCulture(Me)
            MyBase.InitializeCulture()
        End Sub

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If IsPostBack Then Return

            Dim currentUser = UserAccount.FindByEmail(Page.User.Identity.Name)
            If currentUser Is Nothing Then Return

            _isProvider = (currentUser.UserType = "Provider")
            Dim conversations = If(_isProvider,
                DialogMessage.GetConversationsForProvider(currentUser.UserId),
                DialogMessage.GetConversationsForConsumer(currentUser.UserId))

            rptConversations.DataSource = conversations
            rptConversations.DataBind()
            emptyPanel.Visible = (conversations.Count = 0)
        End Sub

        Private _isProvider As Boolean

        ''' <summary>Статус звернення (2026-09-25) — з погляду того, хто дивиться: хто
        ''' написав останнім, той чекає на відповідь іншої сторони.</summary>
        Protected Function StatusText(item As ConversationSummary) As String
            If _isProvider Then
                Return If(item.LastSenderIsConsumer, Resources.SiteText.Messages_Status_NeedsYourReply, Resources.SiteText.Messages_Status_YouReplied)
            End If
            Return If(item.LastSenderIsConsumer, Resources.SiteText.Messages_Status_AwaitingProvider, Resources.SiteText.Messages_Status_ProviderReplied)
        End Function

    End Class

End Namespace
