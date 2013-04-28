Public Class TargetedActions
    Public actions As New List(Of CGAction)
    ' assume the object 'target' have the properties (location, scale...) for actions
    Public target As Object ' will use dynamic binding. 
    Public paused As Boolean
    Public lastItem As TargetedActions = Nothing ' lastItem <--> me <--> nextItem
    Public nextItem As TargetedActions = Nothing
End Class

Public Class CGActionManager
    Private targetActionsHash_ As Hashtable
    Private targetHashHead_ As TargetedActions ' help iterate through the hash table
    Private targetHashTail_ As TargetedActions ' help add new targeted actions

    Sub New()
        targetActionsHash_ = New Hashtable
        targetHashHead_ = Nothing
    End Sub

    Public Sub pauseTarget(ByVal target As Object)
        Debug.Assert(target IsNot Nothing, "target cannot be nil")
        If targetActionsHash_.ContainsKey(target) Then
            Dim ta As TargetedActions = targetActionsHash_.Item(target)
            ta.paused = True
        End If
    End Sub

    Public Sub resumeTarget(ByVal target As Object)
        Debug.Assert(target IsNot Nothing, "target cannot be nil")
        If targetActionsHash_.ContainsKey(target) Then
            Dim ta As TargetedActions = targetActionsHash_.Item(target)
            ta.paused = False
        End If
    End Sub

    Public Sub addAction(ByVal action As CGAction, ByVal target As Object, ByVal paused As Boolean)
        Debug.Assert(action IsNot Nothing, "action cannot be nil")
        Debug.Assert(target IsNot Nothing, "target cannot be nil")
        If Not targetActionsHash_.ContainsKey(target) Then
            Dim ta As New TargetedActions
            ta.target = target
            ta.actions.Add(action)
            ta.paused = paused
            targetActionsHash_.Add(target, ta) ' key , value

            If targetHashHead_ Is Nothing Then
                targetHashHead_ = ta
                targetHashTail_ = ta
            Else
                targetHashTail_.nextItem = ta
                ta.lastItem = targetHashTail_
                targetHashTail_ = ta
            End If
        Else
            Dim ta As TargetedActions = targetActionsHash_.Item(target)
            Debug.Assert(Not ta.actions.Contains(action), "addAction: action already running")
            ta.actions.Add(action)
        End If

        action.startWithTarget(target)
    End Sub

    Public Sub removeAllActions()
        Dim ta As TargetedActions = targetHashHead_
        While ta IsNot Nothing
            Dim target As Object = ta.target
            removeAllActionsFromTarget(target)
            ta = ta.nextItem
        End While
        targetHashHead_ = Nothing
        targetHashTail_ = Nothing
    End Sub

    Public Sub removeAllActionsFromTarget(ByVal target As Object)
        If target Is Nothing Then
            Return
        End If

        If targetActionsHash_.ContainsKey(target) Then
            Dim ta As TargetedActions = targetActionsHash_.Item(target)
            'ta.actions.Clear()
            'targetActionsHash.Remove(target)
            For Each action As CGAction In ta.actions
                action.deletionMark = True
            Next
        End If
    End Sub

    Public Sub removeAction(ByVal action As CGAction)
        If action Is Nothing Then
            Return
        End If

        Dim target As Object = action.target
        If targetActionsHash_.ContainsKey(target) Then
            Dim ta As TargetedActions = targetActionsHash_.Item(target)
            If ta.actions.Contains(action) Then
                action.deletionMark = True
                'removeActionInActionsIfNeedClear(action, ta.actions, target)
            End If
        End If
    End Sub

    ' remove the first action, whose tag matches the first parameter tag.
    ' NOTE: if the target has multiple actions with the same tag, the rest of the actions will not be removed.
    Public Sub removeActionByTag(ByVal tag As Integer, ByVal target As Object)
        Debug.Assert(target IsNot Nothing, "target cannot be nil")

        If targetActionsHash_.ContainsKey(target) Then
            Dim ta As TargetedActions = targetActionsHash_.Item(target)
            For Each action As CGAction In ta.actions
                If action.tag = tag AndAlso Not action.deletionMark Then
                    action.deletionMark = True
                    'removeActionInActionsIfNeedClear(action, ta.actions, target)
                    Exit For
                End If
            Next
        End If
    End Sub

    Private Sub removeActionsIfNeeded(ByVal ta As TargetedActions)
        For i As Integer = 0 To ta.actions.Count - 1
            If ta.actions.Count > 0 Then
                Dim action As CGAction = ta.actions.Item(i)
                If action.deletionMark OrElse action.isDone Then
                    action.stopAction()
                    ta.actions.RemoveAt(i)
                    i -= 1
                End If
            End If
        Next
        If ta.actions.Count = 0 Then
            If ta.lastItem IsNot Nothing Then
                ta.lastItem.nextItem = ta.nextItem
            Else 'ta is the head. remove head
                targetHashHead_ = ta.nextItem
            End If
            If ta.nextItem IsNot Nothing Then
                ta.nextItem.lastItem = ta.lastItem
            Else 'ta is the tail. remove tail
                targetHashTail_ = ta.lastItem
            End If
            targetActionsHash_.Remove(ta.target)
        End If
    End Sub

    ' get the first action
    ' similar with removeActionByTag, it only return the first action matches the tag.
    Public Function getActionByTag(ByVal tag As Integer, ByVal target As Object) As CGAction
        Debug.Assert(target IsNot Nothing, "target cannot be nil")

        Dim action As CGAction = Nothing
        If targetActionsHash_.ContainsKey(target) Then
            Dim ta As TargetedActions = targetActionsHash_.Item(target)
            For Each a As CGAction In ta.actions
                If a.tag = tag AndAlso Not a.deletionMark Then
                    action = a
                    Exit For
                End If
            Next
        End If
        Return action
    End Function

    Public Function numberOfRunningActionsInTarget(ByVal target As Object) As UInteger
        Debug.Assert(target IsNot Nothing, "target cannot be nil")
        Dim number As UInteger = 0
        If targetActionsHash_.ContainsKey(target) Then
            Dim ta As TargetedActions = targetActionsHash_.Item(target)
            number = ta.actions.Count
        End If
        Return number
    End Function

    Public Sub update()
        Dim ta As TargetedActions = targetHashHead_
        While ta IsNot Nothing
            If Not ta.paused Then
                For Each action As CGAction In ta.actions
                    If Not action.deletionMark Then
                        action.takeStep()
                    End If
                Next
            End If
            Dim tempTa As TargetedActions = ta
            ta = ta.nextItem
            removeActionsIfNeeded(tempTa)
        End While
    End Sub
End Class
