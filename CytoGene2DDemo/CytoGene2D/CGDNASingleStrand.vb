Public Class CGDNASingleStrand : Inherits CGNode

#Region "Nodes Properties"
    Private count_ As Integer = 0
    Public ReadOnly Property count As Integer
        Get
            Return count_
        End Get
    End Property

    Private first_ As CGDNANode = Nothing
    Public ReadOnly Property first As CGDNANode
        Get
            Return first_
        End Get
    End Property

    Private last_ As CGDNANode = Nothing
    Public ReadOnly Property last As CGDNANode
        Get
            Return last_
        End Get
    End Property
#End Region

    Private nodeAppendMode_ As NodeAppendMode
    Public ReadOnly Property appendMode As NodeAppendMode
        Get
            Return nodeAppendMode_
        End Get
    End Property

    Private nodePointsList As List(Of List(Of PointF))
    Private colorList As List(Of Color)
    Private isMoving_ As Boolean
    Public Property isMoving As Boolean
        Get
            Return isMoving_
        End Get
        Set(ByVal value As Boolean)
            isMoving_ = value
        End Set
    End Property

    Private defaultColor_ As Color
    Public Property defaultColor As Color ' reset this property when customize
        Get
            Return defaultColor_
        End Get
        Set(ByVal value As Color)
            defaultColor_ = value
            dyeDNAStrand(defaultColor_, 0, count)
            defaultPen_.Color = defaultColor_
        End Set
    End Property

    Private defaultNodeRadius_ As Single
    Public ReadOnly Property defaultNodeRadius As Single
        Get
            Return defaultNodeRadius_
        End Get
    End Property

    Private defaultPen_ As Pen
    Public ReadOnly Property defaultPen As Pen
        Get
            Return defaultPen_
        End Get
    End Property

    Private defaultGap_ As Single ' gap between 2 nodes (Circles)
    Public Property defaultGap As Single
        Get
            Return defaultGap_
        End Get
        Set(value As Single)
            defaultGap_ = value
        End Set
    End Property

    Sub New(ByVal startNodeCenter As PointF, ByVal nodeRadius As Single, ByVal nodeColor As Color,
            ByVal size As Integer, ByVal gap As Single,
            Optional ByVal appendMode As NodeAppendMode = NodeAppendMode.HorizontalLeftRight)
        nodeAppendMode_ = appendMode
        isMoving_ = False
        defaultColor_ = nodeColor
        defaultNodeRadius_ = nodeRadius
        defaultPen_ = New Pen(defaultColor_, defaultNodeRadius_ * 2)
        defaultGap_ = gap
        Dim startingNode As New CGDNANode(nodeRadius, nodeColor, startNodeCenter)
        If count_ = 0 Then
            first_ = startingNode
            last_ = startingNode
            count_ += 1
        End If
        For i As Integer = 1 To size - 1
            appendDNANode()
        Next
    End Sub

#Region "Nodes Operations"
    Public Sub addNodeAfter(ByVal node As CGDNANode, ByVal newNode As CGDNANode)
        If node.nextNode Is Nothing Then ' add a node after the last node
            node.nextNode = newNode
            newNode.lastNode = node
            last_ = newNode
        Else
            newNode.nextNode = node.nextNode
            newNode.nextNode.lastNode = newNode
            node.nextNode = newNode
            newNode.lastNode = node
        End If
        count_ += 1
    End Sub

    Public Sub addNodeBefore(ByVal node As CGDNANode, ByVal newNode As CGDNANode)
        If node.lastNode Is Nothing Then ' add a node before the first node
            node.lastNode = newNode
            newNode.nextNode = node
            first_ = newNode
        Else
            newNode.lastNode = node.lastNode
            newNode.lastNode.nextNode = newNode
            node.lastNode = newNode
            newNode.nextNode = node
        End If
        count_ += 1
    End Sub

    Private Sub appendDNANode()
        Debug.Assert(count > 0, Me.ToString + ": To append a Node, you should at least have one node")
        Dim lastNodeCenter As PointF = last_.center
        Dim newNodeCenter As PointF
        Select Case nodeAppendMode_
            Case NodeAppendMode.HorizontalLeftRight
                newNodeCenter = New PointF(lastNodeCenter.X + 2 * defaultNodeRadius_ + defaultGap_, lastNodeCenter.Y)
            Case NodeAppendMode.HorizontalRightLeft
                newNodeCenter = New PointF(lastNodeCenter.X - 2 * defaultNodeRadius_ + defaultGap_, lastNodeCenter.Y)
            Case NodeAppendMode.VerticalTopBottom
                newNodeCenter = New PointF(lastNodeCenter.X, lastNodeCenter.Y + 2 * defaultNodeRadius_ + defaultGap_)
            Case NodeAppendMode.VerticalBottomTop
                newNodeCenter = New PointF(lastNodeCenter.X, lastNodeCenter.Y - 2 * defaultNodeRadius_ + defaultGap_)
        End Select
        Dim newNode As New CGDNANode(defaultNodeRadius_, defaultColor_, newNodeCenter)
        addNodeAfter(last_, newNode)
    End Sub

    ' operation based on 'Index'. It is a O(n/2) operation
    ' Index start from 0 to count-1
    Public Function getDNANodeAtIndex(ByVal index As Integer) As CGDNANode
        Debug.Assert(index >= 0 And index < count, Me.ToString + ": Invalid index value")
        Dim node As CGDNANode
        If index < count_ \ 2 Then
            node = first_
            While index > 0
                node = node.nextNode
                index -= 1
            End While
        Else
            node = last_
            index = count_ - 1 - index
            While index > 0
                node = node.lastNode
                index -= 1
            End While
        End If
        Return node
    End Function

    Public Sub removeDANNodeAtIndex(ByVal index As Integer)
        Dim nodeToDel = getDNANodeAtIndex(index)
        removeDNANode(nodeToDel)
    End Sub

    ' O(1) operation.
    Private Sub removeDNANode(ByVal nodeToDel As CGDNANode)
        If count <= 0 Then Return
        If nodeToDel.lastNode IsNot Nothing And nodeToDel.nextNode IsNot Nothing Then
            nodeToDel.lastNode.nextNode = nodeToDel.nextNode
            nodeToDel.nextNode.lastNode = nodeToDel.lastNode
        ElseIf nodeToDel.lastNode IsNot Nothing Then
            first_ = nodeToDel.nextNode
            nodeToDel.nextNode.lastNode = Nothing
        ElseIf nodeToDel.nextNode IsNot Nothing Then
            last_ = nodeToDel.lastNode
            nodeToDel.lastNode.nextNode = Nothing
        Else
            first_ = Nothing
            last_ = Nothing
        End If
        count_ -= 1
    End Sub

    Public Sub removeFirst()
        removeDNANode(first_)
    End Sub

    Public Sub removeLast()
        removeDNANode(last_)
    End Sub

    ' insert before the node.
    ' if index == count, insert at the end
    Public Sub insertNodeAtIndex(ByVal index As Integer)
        Debug.Assert(index >= 0 And index <= count, Me.ToString + ": Invalid index value")
        If index < count Then
            Dim node As CGDNANode = getDNANodeAtIndex(index)
            Dim lastNodeCenter As PointF = node.center
            Dim newNodeCenter As PointF
            Select Case nodeAppendMode_
                Case NodeAppendMode.HorizontalLeftRight
                    newNodeCenter = New PointF(lastNodeCenter.X + 2 * defaultNodeRadius_ + defaultGap_, lastNodeCenter.Y)
                Case NodeAppendMode.HorizontalRightLeft
                    newNodeCenter = New PointF(lastNodeCenter.X - 2 * defaultNodeRadius_ + defaultGap_, lastNodeCenter.Y)
                Case NodeAppendMode.VerticalTopBottom
                    newNodeCenter = New PointF(lastNodeCenter.X, lastNodeCenter.Y + 2 * defaultNodeRadius_ + defaultGap_)
                Case NodeAppendMode.VerticalBottomTop
                    newNodeCenter = New PointF(lastNodeCenter.X, lastNodeCenter.Y - 2 * defaultNodeRadius_ + defaultGap_)
            End Select
            Dim newNode As New CGDNANode(defaultNodeRadius_, defaultColor_, newNodeCenter)
            addNodeBefore(node, newNode)
        Else
            appendDNANode()
        End If
    End Sub
#End Region

    ' move routines
    Public Sub moveHeadByDistance(ByVal duration As Integer, ByVal distance As PointF)
        If count = 0 Then Return
        moveNodeAtIndexByDistance(0, duration, distance)
    End Sub

    Public Sub moveTailByDistance(ByVal duration As Integer, ByVal distance As PointF)
        If count = 0 Then Return
        moveNodeAtIndexByDistance(count - 1, duration, distance)
    End Sub

    Public Sub moveHeadToDestination(ByVal duration As Integer, ByVal destination As PointF)
        If count = 0 Then Return
        moveNodeAtIndexToDestination(0, duration, destination)
    End Sub

    Public Sub moveTailToDestination(ByVal duration As Integer, ByVal destination As PointF)
        If count = 0 Then Return
        moveNodeAtIndexToDestination(count - 1, duration, destination)
    End Sub

    ' currently cannot expose this method to public; if move any node in between head and tail, the strand will have strange behavior
    ' TODO: fix it, when require to move body nodes
    ' HINT: have a new kind of follow action.
    ' moveNodeAtIndexToDestination has the same issue.
    Private Sub moveNodeAtIndexByDistance(ByVal index As Integer, ByVal duration As Integer, ByVal distance As PointF) ' drag the node, and other node follows
        Debug.Assert(index >= 0 And index < count, Me.ToString + ": moveByDragNodeAtIndex: index out of boundary")
        Dim node As CGDNANode = getDNANodeAtIndex(index)
        Dim moveBy As New CGMoveBy(duration - 2, distance) ' duration - 2. why -2? delay will take 1 step. And, though, call back take 0 step, but if next step will set another delegate method, conflict happens. To avoid this, expand callback's duration as if it is 1. Therefore -2.
        Dim delay As New CGDelayTime(1) ' delay 1 step. avoid jerky
        Dim callBack As CGActionInstant = CGActionInstant.actionWithDelegate(Sub()
                                                                                 isMoving_ = False
                                                                             End Sub)
        Dim actionSeq As CGSequence = CGSequence.actionWithArray({moveBy, delay, callBack})
        node.runAction(actionSeq)
        makeNodesFollow(node, index)
        isMoving_ = True
    End Sub

    Private Sub moveNodeAtIndexToDestination(ByVal index As Integer, ByVal duration As Integer, ByVal destination As PointF) ' similar with moveByDragNodeAtIndexByDistance
        Debug.Assert(index >= 0 And index < count, Me.ToString + ": moveByDragNodeAtIndex: index out of boundary")
        Dim node As CGDNANode = getDNANodeAtIndex(index)
        Dim moveTo As New CGMoveTo(duration - 2, destination)
        Dim delay As New CGDelayTime(1)
        Dim callBack As CGActionInstant = CGActionInstant.actionWithDelegate(Sub()
                                                                                 isMoving_ = False
                                                                             End Sub)
        Dim actionSeq As CGSequence = CGSequence.actionWithArray({moveTo, delay, callBack})
        node.runAction(actionSeq)
        makeNodesFollow(node, index)
        isMoving_ = True
    End Sub

    Private Sub makeNodesFollow(ByVal node As CGDNANode, ByVal index As Integer)
        Dim nodeRunner As CGDNANode = node
        For i As Integer = index - 1 To 0 Step -1
            Dim follow As New CGFollow(nodeRunner, defaultNodeRadius_ * 2 + defaultGap_)
            nodeRunner.lastNode.runAction(follow)
            nodeRunner = nodeRunner.lastNode
        Next
        nodeRunner = node
        For i As Integer = index + 1 To count - 1
            Dim follow As New CGFollow(nodeRunner, defaultNodeRadius_ * 2 + defaultGap_)
            nodeRunner.nextNode.runAction(follow)
            nodeRunner = nodeRunner.nextNode
        Next
    End Sub

    Public Sub moveEntireStrandByDistance(ByVal duration As Integer, ByVal distance As PointF)
        If count = 0 Then Return
        Dim node As CGDNANode = first
        Dim moveDuration = duration - 2
        Dim moveBy As New CGMoveBy(moveDuration, distance)
        Dim delay As New CGDelayTime(1)
        Dim callBack As CGActionInstant = CGActionInstant.actionWithDelegate(Sub()
                                                                                 isMoving_ = False
                                                                             End Sub)
        Dim actionSeq As CGSequence = CGSequence.actionWithArray({moveBy, delay, callBack})
        node.runAction(actionSeq)
        For i As Integer = 1 To count - 1
            node = node.nextNode
            node.runAction(New CGMoveBy(moveDuration, distance))
        Next
        isMoving_ = True
    End Sub

    Public Sub dyeDNAStrand(ByVal dyeColor As Color, ByVal startIndex As Integer, ByVal length As Integer)
        Debug.Assert(startIndex >= 0 And startIndex < count, Me.ToString + ": dyeDNAStrand: startIndex out of boundary")
        Debug.Assert(length >= 0 And startIndex + length <= count, "dyeDNAStrand: invalid value for length")
        Dim node As CGDNANode = getDNANodeAtIndex(startIndex)
        While length > 0
            node.nodeColor = dyeColor
            node = node.nextNode
            length -= 1
        End While
    End Sub

    Public Overrides Sub draw()
        MyBase.draw()
        Dim context As Graphics = CGDirector.sharedDirector.graphicsContext
        Dim node As CGDNANode = first
        For i As Integer = 0 To count - 2
            node.draw()
            If node.nextNode.nodeColor.ToArgb <> Color.Transparent.ToArgb Then
                context.DrawLine(New Pen(node.nodeColor, defaultNodeRadius_ * 2),
                                 node.center, node.nextNode.center)
                If node.nodeColor.ToArgb <> Color.Transparent.ToArgb Then
                    context.DrawLine(Pens.Black,
                                 node.center, node.nextNode.center)
                End If
            End If
            node = node.nextNode
        Next
        last.draw()
        context = Nothing
    End Sub
End Class

'TODO
'Public Class CGmRNA : Inherits CGDNASingleStrand

'End Class

Public Class CGDNASingleStrandCircular : Inherits CGDNASingleStrand
    Sub New(ByVal circleCenter As PointF, ByVal circleRadius As Single, ByVal nodeRadius As Single,
            ByVal nodeColor As Color, ByVal size As Integer, ByVal gap As Single)
        MyBase.New(PointF.Empty, nodeRadius, nodeColor, size, gap)
        Debug.Assert(size > 3, Me.ToString + ": at least 3 nodes is needed to form a circle")
        makeCircle(circleCenter, circleRadius)
    End Sub

    Private Sub makeCircle(ByVal circleCenter As PointF, ByVal circleRadius As Single)
        Dim aDelta As Single = Math.PI * 2 / (count - 1)
        Dim angle As Single = 0
        Dim node As CGDNANode = first
        For i As Integer = 0 To count - 2
            node.center = New PointF(circleCenter.X + circleRadius * Math.Cos(angle), circleCenter.Y + circleRadius * Math.Sin(angle))
            node = node.nextNode
            angle += aDelta
        Next
        defaultGap = Math.Sqrt(Math.Pow(first.center.X - first.nextNode.center.X, 2) + Math.Pow(first.center.Y - first.nextNode.center.Y, 2)) - defaultNodeRadius * 2
        last.center = first.center
    End Sub
End Class