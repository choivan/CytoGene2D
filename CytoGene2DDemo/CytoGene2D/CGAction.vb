' ------> CGAction <------
Public Class CGAction : Implements ICloneable
    Private target_ As Object
    Public ReadOnly Property target As Object
        Get
            Return target_
        End Get
    End Property
    Public tag As Integer
    Public deletionMark As Boolean = False ' indicate that needs to remove action at the end of the current main loop of action manager

    Public Overridable Sub startWithTarget(ByVal target As Object)
        target_ = target
    End Sub

    ' call every frame
    Public Overridable Sub takeStep()
        ' override me!
    End Sub

    ' called every frame. progress is between 0~1. 
    ' -- 0: action just begin;
    ' -- 0~1: action is in the middel;
    ' -- 1: action is done
    ' DO NOT call it manually
    Public Overridable Sub update(ByVal progress As Single)
        ' override me!
    End Sub

    ' called after the action is Done. It sets the target to nothing.
    ' ! DO NOT call it manually. you should always use CGNodeObject.stopAction
    Public Overridable Sub stopAction()
        target_ = Nothing
    End Sub

    Public Overridable Function isDone() As Boolean
        Return True
    End Function

    Public Overridable Function Clone() As Object Implements ICloneable.Clone
        Dim copy As New CGAction
        copy.tag = tag
        copy.deletionMark = deletionMark
        Return copy
    End Function
End Class



' ------> CGFiniteTimeAction <------
Public Class CGFiniteTimeAction : Inherits CGAction
    Private duration_ As Integer
    Public Property duration As Integer
        Get
            Return duration_
        End Get
        Set(ByVal value As Integer)
            duration_ = value
        End Set
    End Property

    Public Overridable Function reverse() As CGFiniteTimeAction
        Console.WriteLine("CGFiniteTimeAction: reverse. Override me!")
        Return Nothing
    End Function
End Class



' ------> CGInfiniteTimeAction <------
' infinite time action, the action will repeat forever
Public Class CGInfiniteTimeAction : Inherits CGAction
    Private innerAction_ As CGActionInterval
    Public Property innerAction As CGActionInterval
        Get
            Return innerAction_
        End Get
        Set(ByVal value As CGActionInterval)
            innerAction_ = value
        End Set
    End Property

    Shared Function actionWithAction(ByVal action As CGActionInterval) As CGInfiniteTimeAction
        Return New CGInfiniteTimeAction(action)
    End Function

    Sub New(ByVal action As CGActionInterval)
        innerAction_ = action
    End Sub

    Public Overrides Function Clone() As Object
        Dim copy As CGAction = New CGInfiniteTimeAction(innerAction_)
        Return copy
    End Function

    Public Overrides Sub startWithTarget(ByVal target As Object)
        MyBase.startWithTarget(target)
        innerAction_.startWithTarget(target)
    End Sub

    Public Overrides Sub takeStep()
        innerAction_.takeStep()
        If innerAction_.isDone Then
            innerAction_.startWithTarget(target)
        End If
    End Sub

    Public Overrides Function isDone() As Boolean
        Return False
    End Function

    Public Function reverse() As CGInfiniteTimeAction
        Return New CGInfiniteTimeAction(innerAction_.reverse)
    End Function

End Class


' ------> CGFollow <------
' node follows the previous node
Public Class CGFollow : Inherits CGAction
    Private followedNode_ As CGNode
    Private distance_ As Single

    Sub New(ByVal fNode As CGNode, ByVal distance As Single)
        followedNode_ = fNode
        distance_ = distance
    End Sub

    Public Overrides Sub takeStep()
        approachToLeadingNode(target, followedNode_)
    End Sub

    Private Sub approachToLeadingNode(ByVal aNode As Object, ByVal bNode As Object)
        Dim dx As Single = bNode.center.X - aNode.center.X
        Dim dy As Single = bNode.center.Y - aNode.center.Y
        Dim dD As Single = Math.Sqrt(dx * dx + dy * dy)
        Dim ratio As Single = distance_ / dD
        aNode.center = New PointF(bNode.center.X - ratio * dx, bNode.center.Y - ratio * dy)
    End Sub

    Public Overrides Function isDone() As Boolean
        Return followedNode_.numberOfRunningActions() = 0
    End Function

End Class

' CGFollowForever action will remain running until is manually removed
Public Class CGFollowForever : Inherits CGFollow
    Sub New(ByVal fNode As CGNode, ByVal distance As Single)
        MyBase.New(fNode, distance)
    End Sub

    Public Overrides Function isDone() As Boolean
        Return False
    End Function
End Class