Public Class CGActionInterval : Inherits CGFiniteTimeAction
    Private elapsed_ As Integer
    Public ReadOnly Property elapsed As Integer
        Get
            Return elapsed_
        End Get
    End Property
    Private firstTick_ As Boolean

    Sub New(ByVal duration As Integer)
        firstTick_ = True
        Me.duration = duration
        elapsed_ = 0
    End Sub

    Public Overrides Function Clone() As Object
        Return New CGActionInterval(Me.duration)
    End Function

    Public Overrides Function isDone() As Boolean
        Return elapsed_ >= Me.duration
    End Function

    Public Overrides Sub startWithTarget(ByVal target As Object)
        MyBase.startWithTarget(target)
        elapsed_ = 0
    End Sub

    Public Overrides Sub takeStep()
        If firstTick_ Then
            firstTick_ = False
            elapsed_ = 0
        Else
            elapsed_ += 1
        End If
        update(Math.Max(0,
                        Math.Min(1,
                                 elapsed_ / Math.Max(duration,
                                                     Single.Epsilon)))) ' using single.epsilon to avoid division by 0
    End Sub

    Public Overrides Function reverse() As CGFiniteTimeAction
        Debug.Assert(False, "CGActionInterval: reverse not implemented") ' note. the condition is set to false directly
        Return Nothing
    End Function
End Class

Public Class CGDelayTime : Inherits CGActionInterval
    Sub New(ByVal duration As Integer)
        MyBase.New(duration)
    End Sub

    Public Overrides Sub update(ByVal progress As Single)
        Return
    End Sub

    Public Overrides Function reverse() As CGFiniteTimeAction
        Return new CGDelayTime(duration)
    End Function
End Class

Public Class CGMoveTo : Inherits CGActionInterval
    Private delta_ As PointF
    Private endPosition_ As PointF
    Private startPosition_ As PointF
    Public Property delta As PointF
        Get
            Return delta_
        End Get
        Set(ByVal value As PointF)
            delta_ = value
        End Set
    End Property
    Public ReadOnly Property endPosition As PointF
        Get
            Return endPosition_
        End Get
    End Property
    Public ReadOnly Property startPosition As PointF
        Get
            Return startPosition_
        End Get
    End Property

    Sub New(ByVal duration As Integer, ByVal location As PointF)
        MyBase.New(duration)
        endPosition_ = location
    End Sub

    Public Overrides Function Clone() As Object
        Return New CGMoveTo(duration, endPosition_)
    End Function

    Public Overrides Sub startWithTarget(ByVal target As Object)
        MyBase.startWithTarget(target)
        startPosition_ = target.center
        delta_ = New PointF(endPosition_.X - startPosition_.X, endPosition_.Y - startPosition_.Y)
    End Sub

    Public Overrides Sub update(ByVal progress As Single)
        target.center = New PointF(startPosition_.X + progress * delta_.X, startPosition_.Y + progress * delta_.Y)
    End Sub
End Class

Public Class CGMoveBy : Inherits CGMoveTo

    Sub New(ByVal duration As Integer, ByVal delta As PointF)
        MyBase.New(duration, Nothing)
        Me.delta = delta
    End Sub

    Public Overrides Function Clone() As Object
        Return New CGMoveBy(duration, delta)
    End Function

    Public Overrides Sub startWithTarget(ByVal target As Object)
        Dim tempDelta As PointF = Me.delta
        MyBase.startWithTarget(target) ' startWithTarget in base class will override the delta
        Me.delta = tempDelta
    End Sub

    Public Overrides Function reverse() As CGFiniteTimeAction
        Return New CGMoveBy(duration, New PointF(-delta.X, -delta.Y))
    End Function
End Class

Public Class CGFadeTo : Inherits CGActionInterval
    Private toOpacity_ As Byte
    Private fromOpacity_ As Byte

    Sub New(ByVal duration As Integer, ByVal opacity As Byte)
        MyBase.New(duration)
        toOpacity_ = opacity
    End Sub

    Public Overrides Function Clone() As Object
        Return New CGFadeTo(duration, toOpacity_)
    End Function

    Public Overrides Sub startWithTarget(ByVal target As Object)
        MyBase.startWithTarget(target)
        fromOpacity_ = target.opacity
    End Sub

    Public Overrides Sub update(ByVal progress As Single)
        target.opacity = fromOpacity_ + (CInt(toOpacity_) - CInt(fromOpacity_)) * progress
    End Sub
End Class

Public Class CGFadeIn : Inherits CGActionInterval
    Sub New(ByVal duration As Integer)
        MyBase.New(duration)
    End Sub

    Public Overrides Sub update(ByVal progress As Single)
        target.opacity = progress * Byte.MaxValue
    End Sub

    Public Overrides Function reverse() As CGFiniteTimeAction
        Return New CGFadeOut(duration)
    End Function
End Class

Public Class CGFadeOut : Inherits CGActionInterval
    Sub New(ByVal duration As Integer)
        MyBase.New(duration)
    End Sub

    Public Overrides Sub update(ByVal progress As Single)
        target.opacity = (1 - progress) * Byte.MaxValue
    End Sub

    Public Overrides Function reverse() As CGFiniteTimeAction
        Return New CGFadeIn(duration)
    End Function
End Class

' combine 2 actions into a sequence
' or combine a list of actions by calling the class method <CGSequence.actionWithArray>
Public Class CGSequence : Inherits CGActionInterval
    Private action1_ As CGFiniteTimeAction
    Private action2_ As CGFiniteTimeAction
    Private split_ As Single
    Private last_ As Integer

    Sub New(ByVal action1 As CGFiniteTimeAction, ByVal action2 As CGFiniteTimeAction)
        MyBase.New(action1.duration + action2.duration)
        Debug.Assert(action1 IsNot Nothing And action2 IsNot Nothing, "Sequence: arguments cannot be nothing")
        Debug.Assert(action1 IsNot action1_ And action2 IsNot action2_, "Sequence: cannot re-init using the same parameter")
        Debug.Assert(action1 IsNot action2_ And action2 IsNot action1_, "Sequence: cannot re-init using the same parameter")
        action1_ = action1
        action2_ = action2
    End Sub

    ' THIS METHOD IS NOT SAFE! LACKING OF VALIDATION OF PARAMETERS! USE 'actionWithArray' instead.
    <Obsolete("Don't use this method any more. Use the the class method <CGSequence.actionWithArray> instead.")>
    Sub New(ByVal actions() As CGFiniteTimeAction)
        MyBase.New(0)
        Debug.Assert(actions.Length > 1, "Sequence: at least 2 actions are needed")
        action1_ = actions(0)
        For i As Integer = 1 To actions.Length - 1
            action1_ = New CGSequence(action1_, actions(i))
        Next
        action2_ = actions(actions.Length - 1)
        Me.duration = action1_.duration + action2_.duration
    End Sub

    ' the interface to create a sequence of actions from a list of actions
    Shared Function actionWithArray(ByVal actions As List(Of CGFiniteTimeAction)) As CGSequence
        Debug.Assert(actions.Count > 1, "Sequence: at least 2 actions are needed")
        Dim preAction As CGFiniteTimeAction = actions.Item(0)
        For i As Integer = 1 To actions.Count - 1
            preAction = New CGSequence(preAction, actions.Item(i))
        Next
        Return preAction
    End Function

    ' CGSequence.actionWithArray({action1, action2, ..., actionN})
    Shared Function actionWithArray(ByVal actions() As CGFiniteTimeAction) As CGSequence
        Debug.Assert(actions.Length > 1, "Sequence: at least 2 actions are needed")
        Dim preAction As CGFiniteTimeAction = actions(0)
        For i As Integer = 1 To actions.Length - 1
            preAction = New CGSequence(preAction, actions(i))
        Next
        Return preAction
    End Function

    Public Overrides Function Clone() As Object
        Return New CGSequence(action1_.Clone, action2_.Clone)
    End Function

    Public Overrides Sub startWithTarget(ByVal target As Object)
        MyBase.startWithTarget(target)
        split_ = action1_.duration / Math.Max(duration, Single.Epsilon)
        last_ = -1
    End Sub

    Public Overrides Sub stopAction()
        If last_ = 0 Then
            action1_.stopAction()
        ElseIf last_ = 1 Then
            action2_.stopAction()
        End If

        MyBase.stopAction()
    End Sub

    Public Overrides Sub update(ByVal progress As Single)
        Dim found As Integer = 0
        Dim new_progress As Single = 0
        If progress < split_ Then ' action1_
            found = 0
            If split_ <> 0 Then
                new_progress = progress / split_
            Else ' ation1_ is an instant action
                new_progress = 1
            End If
        Else ' action2_
            found = 1
            If split_ = 1 Then ' action2_ is an instant action
                new_progress = 1
            Else
                new_progress = (progress - split_) / (1 - split_)
            End If
        End If

        If found = 1 Then
            If last_ = -1 Then 'ops! somehow the action1_ is skipped
                action1_.startWithTarget(target)
                action1_.update(1.0F)
                action1_.stopAction()
            ElseIf last_ = 0 Then ' the current action is actually action2_. so stop action1_
                action1_.update(1.0F)
                action1_.stopAction()
            End If
        End If

        If found <> last_ Then
            If found = 0 Then
                action1_.startWithTarget(target)
            Else
                action2_.startWithTarget(target)
            End If
        End If

        If found = 0 Then
            action1_.update(new_progress)
        Else
            action2_.update(new_progress)
        End If
        last_ = found
    End Sub

    Public Overrides Function reverse() As CGFiniteTimeAction
        Return New CGSequence(action2_.reverse, action1_.reverse)
    End Function

End Class

Public Class CGSpawn : Inherits CGActionInterval
    Private action1_ As CGFiniteTimeAction
    Private action2_ As CGFiniteTimeAction

    Sub New(ByVal action1 As CGFiniteTimeAction, ByVal action2 As CGFiniteTimeAction)
        MyBase.New(Math.Max(action1.duration, action2.duration))
        Debug.Assert(action1 IsNot Nothing And action2 IsNot Nothing, "Sequence: arguments cannot be nothing")
        Debug.Assert(action1 IsNot action1_ And action2 IsNot action2_, "Sequence: cannot re-init using the same parameter")
        Debug.Assert(action1 IsNot action2_ And action2 IsNot action1_, "Sequence: cannot re-init using the same parameter")
        action1_ = action1
        action2_ = action2
        If action1_.duration > action2_.duration Then
            action2_ = New CGSequence(action2_, New CGDelayTime(action1_.duration - action2_.duration))
        ElseIf action1_.duration < action2_.duration Then
            action1_ = New CGSequence(action1_, New CGDelayTime(action2_.duration - action1_.duration))
        End If
    End Sub

    Public Overrides Function Clone() As Object
        Return New CGSpawn(action1_, action2_)
    End Function

    Public Overrides Sub startWithTarget(ByVal target As Object)
        MyBase.startWithTarget(target)
        action1_.startWithTarget(target)
        action2_.startWithTarget(target)
    End Sub

    Public Overrides Sub stopAction()
        action1_.stopAction()
        action2_.stopAction()
        MyBase.stopAction()
    End Sub

    Public Overrides Sub update(ByVal progress As Single)
        action1_.update(progress)
        action2_.update(progress)
    End Sub

    Public Overrides Function reverse() As CGFiniteTimeAction
        Return New CGSpawn(action1_.reverse, action2_.reverse)
    End Function
End Class

' CGRepeat different from CGInfiniteTimeAction. CGRepeat has a number of repeat times.
Public Class CGRepeat : Inherits CGActionInterval
    Private times_ As UInteger
    Private total_ As UInteger
    Private innerAction_ As CGFiniteTimeAction

    Sub New(ByVal action As CGFiniteTimeAction, ByVal times As UInteger)
        MyBase.New(action.duration * times) ' instant action has a duration of 0
        times_ = times
        innerAction_ = action
        total_ = 0
    End Sub

    Public Overrides Function Clone() As Object
        Return New CGRepeat(innerAction_.Clone, times_)
    End Function

    Public Overrides Sub startWithTarget(ByVal target As Object)
        total_ = 0
        MyBase.startWithTarget(target)
        innerAction_.startWithTarget(target)
    End Sub

    Public Overrides Sub stopAction()
        innerAction_.stopAction()
        MyBase.stopAction()
    End Sub

    Public Overrides Sub update(ByVal progress As Single)
        Dim p As Single = progress * times_
        If p > total_ + 1 Then
            innerAction_.update(1.0F)
            total_ += 1
            innerAction_.stopAction()
            innerAction_.startWithTarget(target)

            If total_ = times_ Then
                innerAction_.update(0)
            Else
                innerAction_.update(p - total_)
            End If
        Else
            Dim r As Single = p Mod 1.0F
            If progress = 1.0F Then
                r = 1.0F
                total_ += 1
            End If
            innerAction_.update(Math.Min(r, 1))
        End If
    End Sub

    Public Overrides Function isDone() As Boolean
        Return total_ = times_
    End Function
End Class

' blink modifies its visible property
Public Class CGBlink : Inherits CGActionInterval
    Private times_ As Integer

    Sub New(ByVal duration As Integer, ByVal blinks As Integer)
        MyBase.New(duration)
        times_ = blinks
    End Sub

    Public Overrides Function Clone() As Object
        Return New CGBlink(duration, times_)
    End Function

    Public Overrides Sub update(ByVal progress As Single)
        If Not isDone() Then
            Dim slice As Single = 1.0F / times_
            Dim m As Single = progress Mod slice
            target.visible = IIf(m < slice / 2, True, False)
        End If
    End Sub
End Class

Public Class CGScaleTo : Inherits CGActionInterval
    Private delta_ As Single
    Public Property delta As Single
        Get
            Return delta_
        End Get
        Set(ByVal value As Single)
            delta_ = value
        End Set
    End Property
    Private startScale_ As Single
    Private endScale_ As Single

    Sub New(ByVal duration As Integer, ByVal scale As Single)
        MyBase.New(duration)
        endScale_ = scale
    End Sub

    Public Overrides Sub startWithTarget(ByVal target As Object)
        MyBase.startWithTarget(target)
        startScale_ = target.scale
        delta_ = endScale_ - startScale_
    End Sub

    Public Overrides Function Clone() As Object
        Return new CGScaleTo(duration, endScale_)
    End Function

    Public Overrides Sub update(ByVal progress As Single)
        target.scale = startScale_ + progress * delta_
    End Sub
End Class

Public Class CGScaleBy : Inherits CGScaleTo

    Sub New(ByVal duration As Integer, ByVal delta As Single)
        MyBase.New(duration, 0)
        Me.delta = delta
    End Sub

    Public Overrides Sub startWithTarget(ByVal target As Object)
        Dim tempDelta As Single = Me.delta
        MyBase.startWithTarget(target)
        Me.delta = tempDelta
    End Sub

    Public Overrides Function Clone() As Object
        Return New CGScaleBy(duration, Me.delta)
    End Function

    Public Overrides Function reverse() As CGFiniteTimeAction
        Return New CGScaleBy(duration, -Me.delta)
    End Function
End Class

Public Class CGRotateTo : Inherits CGActionInterval
    Private delta_ As Single
    Public Property delta As Single
        Get
            Return delta_
        End Get
        Set(ByVal value As Single)
            delta_ = value
        End Set
    End Property
    Private startAngle_ As Single
    Private endAngle_ As Single

    Sub New(ByVal duration As Integer, ByVal angle As Single)
        MyBase.New(duration)
        endAngle_ = angle Mod 361.0F
    End Sub

    Public Overrides Sub startWithTarget(ByVal target As Object)
        MyBase.startWithTarget(target)
        startAngle_ = target.rotation
        delta_ = endAngle_ - startAngle_
    End Sub

    Public Overrides Function Clone() As Object
        Return New CGRotateTo(duration, endAngle_)
    End Function

    Public Overrides Sub update(ByVal progress As Single)
        target.rotation = startAngle_ + progress * delta_
    End Sub
End Class

Public Class CGRotateBy : Inherits CGRotateTo

    Sub New(ByVal duration As Integer, ByVal delta As Single)
        MyBase.New(duration, 0)
        Me.delta = delta Mod 361.0F
    End Sub

    Public Overrides Sub startWithTarget(ByVal target As Object)
        Dim tempDelta As Single = Me.delta
        MyBase.startWithTarget(target)
        Me.delta = tempDelta
    End Sub

    Public Overrides Function Clone() As Object
        Return New CGRotateBy(duration, Me.delta)
    End Function

    Public Overrides Function reverse() As CGFiniteTimeAction
        Return New CGRotateBy(duration, -Me.delta)
    End Function
End Class