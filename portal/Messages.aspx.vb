Imports System

Namespace SumyPortal

    ''' <summary>Список розмов поточного користувача (п.10 уточненої постановки) — своя
    ''' вибірка для Consumer і для Provider (один може писати з кількох оголошень,
    ''' другому може писати кілька різних споживачів по одному оголошенню).</summary>
    Public Class Messages
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If IsPostBack Then Return

            Dim currentUser = UserAccount.FindByEmail(Page.User.Identity.Name)
            If currentUser Is Nothing Then Return

            Dim conversations = If(currentUser.UserType = "Provider",
                DialogMessage.GetConversationsForProvider(currentUser.UserId),
                DialogMessage.GetConversationsForConsumer(currentUser.UserId))

            rptConversations.DataSource = conversations
            rptConversations.DataBind()
            emptyPanel.Visible = (conversations.Count = 0)
        End Sub

    End Class

End Namespace
