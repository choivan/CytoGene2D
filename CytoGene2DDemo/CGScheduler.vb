' Could be an interesting concept. Currently not in use.
'  A native timer is now used in CGDirector
'Public Class CGTimer
'    Private target_ As Object
'    Private tickElapsed_ As Integer
'    Private tickDelay_ As Integer

'    Private WithEvents timer_ As System.Windows.Forms.Timer

'    Public interval As Single
'    Public Delegate Sub UpdateMethod()
'    Public update As UpdateMethod

'    Sub New(ByVal update As UpdateMethod)
'        MyClass.New(update, 0.05, 0)
'    End Sub

'    Sub New(ByVal update As UpdateMethod, ByVal interval As Single)
'        MyClass.New(update, interval, 0)
'    End Sub

'    Sub New(ByVal update As UpdateMethod, ByVal interval As Single, ByVal delay As Integer)
'        Me.update = update
'        Me.interval = interval
'        Me.tickDelay_ = delay
'        timer_ = New Windows.Forms.Timer()
'        timer_.Interval = interval * 1000
'        timer_.Start()
'    End Sub

'    Public Sub stopTimer()
'        timer_.Stop()
'    End Sub

'    Public Sub startTimer()
'        timer_.Start()
'    End Sub

'    Private Sub timer__Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles timer_.Tick
'        If tickDelay_ > 0 Then
'            tickDelay_ -= 1
'        Else
'            update.Invoke()
'        End If
'    End Sub
'End Class

Public Class TargetedUpdate
    Public paused As Boolean
    Public priority As Integer
    Public target As Object
    Delegate Sub updateDelegate()
    Public update As updateDelegate

    Public deletionMark As Boolean = False
    Public nextTU As TargetedUpdate = Nothing
    Public lastTU As TargetedUpdate = Nothing

    Sub New(ByVal target As Object, ByVal priority As Integer, ByVal paused As Boolean) 'target object should implement update method!!!, if calling this constructor
        Me.target = target
        Me.paused = paused
        Me.priority = priority
        Me.update = AddressOf target.update
    End Sub

    'Sub New(ByVal target As Object, ByVal priority As Integer, ByVal paused As Boolean, ByVal update As updateDelegate) ' reserved interface for custom update method. NOT IN USE
    '    Me.target = target
    '    Me.paused = paused
    '    Me.priority = priority
    '    Me.update = update
    'End Sub
End Class

Public Class CGScheduler
    ' different arrays to place an update task. Priority negtive >> zero >> positive
    Private targetedUpdatesPriorityZero_ As TargetedUpdate
    Private targetedUpdatesPriorityNegative_ As TargetedUpdate
    Private targetedUpdatesPriorityPositive_ As TargetedUpdate
    Private listRunner_ As TargetedUpdate
    Private updatesHash As Hashtable ' to check if an update has already be scheduled

    Sub New()
        updatesHash = New Hashtable
        targetedUpdatesPriorityNegative_ = Nothing
        targetedUpdatesPriorityZero_ = Nothing
        targetedUpdatesPriorityPositive_ = Nothing

    End Sub

    Public Sub scheduleUpdate(ByVal target As Object, ByVal priority As Integer, ByVal paused As Boolean)
        If updatesHash.ContainsKey(target) Then
            Debug.Assert(False, "CGScheduler: cannot re-schedule an update method. Unschedule it first")
            Return
        End If

        addTargetToList(target, priority, paused)
    End Sub

    Private Sub addTargetToList(ByVal target As Object, ByVal priority As Integer, ByVal paused As Boolean)
        Dim tu As New TargetedUpdate(target, priority, paused)
        updatesHash.Add(target, tu)

        If priority = 0 Then
            If targetedUpdatesPriorityZero_ Is Nothing Then
                targetedUpdatesPriorityZero_ = tu
            Else
                listRunner_ = targetedUpdatesPriorityZero_
                While listRunner_.nextTU IsNot Nothing ' go to the end
                    listRunner_ = listRunner_.nextTU
                End While
                listRunner_.nextTU = tu
                tu.lastTU = listRunner_
            End If
        ElseIf priority > 0 Then
            If targetedUpdatesPriorityPositive_ Is Nothing Then
                targetedUpdatesPriorityPositive_ = tu
            Else
                listRunner_ = targetedUpdatesPriorityPositive_
                While listRunner_ IsNot Nothing
                    If tu.priority < listRunner_.priority Then ' insert tu before listRunner
                        Dim lastTU As TargetedUpdate = listRunner_.lastTU
                        If lastTU IsNot Nothing Then
                            lastTU.nextTU = tu
                        Else
                            targetedUpdatesPriorityPositive_ = tu ' insert before the first node, tu will be the first
                        End If
                        tu.lastTU = lastTU
                        tu.nextTU = listRunner_
                        listRunner_.lastTU = tu
                        Return
                    End If
                    If listRunner_.nextTU Is Nothing Then
                        listRunner_.nextTU = tu
                        tu.lastTU = listRunner_
                        Return
                    Else
                        listRunner_ = listRunner_.nextTU
                    End If
                End While

            End If
        Else
            If targetedUpdatesPriorityNegative_ Is Nothing Then
                targetedUpdatesPriorityNegative_ = tu
            Else
                listRunner_ = targetedUpdatesPriorityNegative_
                While listRunner_ IsNot Nothing
                    If tu.priority < listRunner_.priority Then
                        Dim lastTU As TargetedUpdate = listRunner_.lastTU
                        If lastTU IsNot Nothing Then
                            lastTU.nextTU = tu
                        Else
                            targetedUpdatesPriorityNegative_ = tu
                        End If
                        tu.lastTU = lastTU
                        tu.nextTU = listRunner_
                        listRunner_.lastTU = tu
                        Return
                    End If
                    If listRunner_.nextTU Is Nothing Then
                        listRunner_.nextTU = tu
                        tu.lastTU = listRunner_
                        Return
                    Else
                        listRunner_ = listRunner_.nextTU
                    End If
                End While
            End If
        End If
    End Sub

    Public Sub unscheduleUpdate(ByVal target As Object)
        If target Is Nothing Then
            Return
        End If

        If updatesHash.Contains(target) Then
            Dim tu As TargetedUpdate = updatesHash.Item(target)
            tu.deletionMark = True
        End If
    End Sub

    Private Sub removeUpdateIfNeeded(ByVal tu As TargetedUpdate)
        If tu.deletionMark Then
            If tu.lastTU IsNot Nothing Then
                tu.lastTU.nextTU = tu.nextTU
            Else
                If tu.priority = 0 Then
                    targetedUpdatesPriorityZero_ = tu.nextTU
                ElseIf tu.priority < 0 Then
                    targetedUpdatesPriorityNegative_ = tu.nextTU
                Else
                    targetedUpdatesPriorityPositive_ = tu.nextTU
                End If
            End If
            If tu.nextTU IsNot Nothing Then
                tu.nextTU.lastTU = tu.lastTU
            End If
        End If

        updatesHash.Remove(tu.target)
        tu.nextTU = Nothing
        tu.lastTU = Nothing
        tu = Nothing
    End Sub

    Public Sub pauseTarget(ByVal target As Object)
        If target Is Nothing Then
            Return
        End If

        If updatesHash.Contains(target) Then
            Dim tu As TargetedUpdate = updatesHash.Item(target)
            tu.paused = True
        End If
    End Sub

    Public Sub resumeTarget(ByVal target As Object)
        If target Is Nothing Then
            Return
        End If

        If updatesHash.Contains(target) Then
            Dim tu As TargetedUpdate = updatesHash.Item(target)
            tu.paused = False
        End If
    End Sub

    Public Function isTargetPaused(ByVal target As Object) As Boolean
        Debug.Assert(target IsNot Nothing, "Target cannot be nil")

        If updatesHash.Contains(target) Then
            Dim tu As TargetedUpdate = updatesHash.Item(target)
            Return tu.paused
        End If
        Return False
    End Function

    Public Sub update()
        ' updates with priority < 0
        listRunner_ = targetedUpdatesPriorityNegative_
        While listRunner_ IsNot Nothing
            listRunner_.update.Invoke()
            listRunner_ = listRunner_.nextTU
        End While
        ' updates with priority = 0
        listRunner_ = targetedUpdatesPriorityZero_
        While listRunner_ IsNot Nothing
            listRunner_.update.Invoke()
            listRunner_ = listRunner_.nextTU
        End While
        ' updates with priority > 0
        listRunner_ = targetedUpdatesPriorityPositive_
        While listRunner_ IsNot Nothing
            listRunner_.update.Invoke()
            listRunner_ = listRunner_.nextTU
        End While

        ' deletion
        listRunner_ = targetedUpdatesPriorityNegative_
        While listRunner_ IsNot Nothing
            Dim tu As TargetedUpdate = listRunner_
            listRunner_ = listRunner_.nextTU
            removeUpdateIfNeeded(tu)
        End While
        listRunner_ = targetedUpdatesPriorityZero_
        While listRunner_ IsNot Nothing
            Dim tu As TargetedUpdate = listRunner_
            listRunner_ = listRunner_.nextTU
            removeUpdateIfNeeded(tu)
        End While
        listRunner_ = targetedUpdatesPriorityPositive_
        While listRunner_ IsNot Nothing
            Dim tu As TargetedUpdate = listRunner_
            listRunner_ = listRunner_.nextTU
            removeUpdateIfNeeded(tu)
        End While
    End Sub

#Region "Debug"
    Public Sub printAllScheduledUpdates()
        Console.WriteLine("Tasks with negative priority")
        listRunner_ = targetedUpdatesPriorityNegative_
        While listRunner_ IsNot Nothing
            Console.WriteLine("Target: " + listRunner_.target.ToString + "; priority: " + listRunner_.priority.ToString)
            listRunner_ = listRunner_.nextTU
        End While
        Console.WriteLine("============================")
        Console.WriteLine("Tasks with zero priority")
        listRunner_ = targetedUpdatesPriorityZero_
        While listRunner_ IsNot Nothing
            Console.WriteLine("Target: " + listRunner_.target.ToString + "; priority: " + listRunner_.priority.ToString)
            listRunner_ = listRunner_.nextTU
        End While
        Console.WriteLine("============================")
        Console.WriteLine("Tasks with positive priority")
        listRunner_ = targetedUpdatesPriorityPositive_
        While listRunner_ IsNot Nothing
            Console.WriteLine("Target: " + listRunner_.target.ToString + "; priority: " + listRunner_.priority.ToString)
            listRunner_ = listRunner_.nextTU
        End While
        Console.WriteLine("============================")
    End Sub

    Public Sub testRemoveUpdates()
        listRunner_ = targetedUpdatesPriorityNegative_
        While listRunner_ IsNot Nothing
            Dim tu As TargetedUpdate = listRunner_
            listRunner_ = listRunner_.nextTU
            removeUpdateIfNeeded(tu)
        End While
        listRunner_ = targetedUpdatesPriorityZero_
        While listRunner_ IsNot Nothing
            Dim tu As TargetedUpdate = listRunner_
            listRunner_ = listRunner_.nextTU
            removeUpdateIfNeeded(tu)
        End While
        listRunner_ = targetedUpdatesPriorityPositive_
        While listRunner_ IsNot Nothing
            Dim tu As TargetedUpdate = listRunner_
            listRunner_ = listRunner_.nextTU
            removeUpdateIfNeeded(tu)
        End While
    End Sub
#End Region
End Class