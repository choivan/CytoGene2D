Public Class TargetedInteraction
    Public interaction As CGInteraction ' one target will only have 1 interaction at a time
    Public target As Object
    Public nextItem As TargetedInteraction = Nothing
    Public lastItem As TargetedInteraction = Nothing
End Class

Public Class CGInteractionManager
    Private interactionsHash_ As Hashtable
    Private hashHead_ As TargetedInteraction
    Private hashTail_ As TargetedInteraction

    Sub New()
        interactionsHash_ = New Hashtable
        hashHead_ = Nothing : hashTail_ = Nothing
    End Sub

    Public Sub addInteraction(ByVal interaction As CGInteraction, ByVal target As Object)
        Debug.Assert(interaction IsNot Nothing, "interaction cannot be nil")
        Debug.Assert(target IsNot Nothing, "target cannot be nil")

        If Not interactionsHash_.ContainsKey(target) Then
            Dim ti As New TargetedInteraction
            ti.target = target
            ti.interaction = interaction
            interactionsHash_.Add(target, ti) ' key , value

            If hashHead_ Is Nothing Then
                hashHead_ = ti
                hashTail_ = ti
            Else
                hashTail_.nextItem = ti
                ti.lastItem = hashTail_
                hashTail_ = ti
            End If
        Else
            Dim ti As TargetedInteraction = interactionsHash_.Item(target)
            Debug.Assert(ti.interaction IsNot interaction, "addAction: interaction already added")
            ti.interaction = interaction
        End If

        interaction.startWithTarget(target)
    End Sub

    Public Sub removeAllInteractions()
        Dim ti As TargetedInteraction = hashHead_
        While ti IsNot Nothing
            Dim target As Object = ti.target
            removeInteractionFromTarget(target)
            ti = ti.nextItem
        End While
        hashHead_ = Nothing
        hashTail_ = Nothing
    End Sub

    Public Sub removeInteractionFromTarget(ByVal target As Object)
        If target Is Nothing Then
            Return
        End If

        If interactionsHash_.ContainsKey(target) Then
            Dim ti As TargetedInteraction = interactionsHash_.Item(target)
            If ti.lastItem IsNot Nothing Then
                ti.lastItem.nextItem = ti.nextItem
            Else 'ti is head. remove head
                hashHead_ = ti.nextItem
            End If
            If ti.nextItem IsNot Nothing Then
                ti.nextItem.lastItem = ti.lastItem
            Else 'ti is tail. remove tail
                hashTail_ = ti.nextItem
            End If
            interactionsHash_.Remove(ti.target)
        End If
    End Sub

    Public Sub update(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs, ByVal m As MouseEvent)
        Dim ti As TargetedInteraction = hashHead_
        While ti IsNot Nothing
            ti.interaction.update(sender, e, m)
            If ti.interaction.isDone Then
                Dim tempTI As TargetedInteraction = ti
                ti = ti.nextItem
                removeInteractionFromTarget(tempTI.target)
            Else
                ti = ti.nextItem
            End If
        End While
    End Sub
End Class
