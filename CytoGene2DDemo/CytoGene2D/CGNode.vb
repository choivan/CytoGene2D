Public Class CGNode
    Private isReorderChildDirty_ As Boolean
    Private actionManager_ As CGActionManager
    Private scheduler_ As CGScheduler
    Private interactionManager_ As CGInteractionManager

    Private location_ As PointF ' upper left point
    Private center_ As PointF ' center point
    Private contentSize_ As SizeF
    Private originalContentSize_ As SizeF
    Public Property location As PointF
        Get
            Return location_
        End Get
        Set(ByVal value As PointF)
            location_ = value
            center_ = New PointF(location_.X + contentSize_.Width / 2, location_.Y + contentSize_.Height / 2)
        End Set
    End Property
    Public Property center As PointF
        Get
            Return center_
        End Get
        Set(ByVal value As PointF)
            center_ = value
            location_ = New PointF(center_.X - contentSize_.Width / 2, center_.Y - contentSize_.Width / 2)
        End Set
    End Property
    Public Property contentSize As SizeF
        Get
            Return contentSize_
        End Get
        Set(ByVal value As SizeF)
            originalContentSize_ = value
            contentSize_ = value
            ' keep location the same
            ' location = location_
            ' keep center the same 
            center = center_
        End Set
    End Property
    Private zOrder_ As Integer
    Public Property zOrder As Integer
        Get
            Return zOrder_
        End Get
        Set(ByVal value As Integer)
            zOrder_ = value
            If parent IsNot Nothing Then
                parent.reorderChild(Me, value)
            End If
        End Set
    End Property
    Private scale_, scaleX_, scaleY_ As Single
    Public Property scale As Single
        Get
            Return scale_
        End Get
        Set(ByVal value As Single)
            scale_ = value
            scaleX_ = value
            scaleY_ = value
            contentSize_ = New SizeF(originalContentSize_.Width * scaleX_, originalContentSize_.Height * scaleY_)
            center = center_ ' keep center the same, location will automatically changed
        End Set
    End Property
    Public Property scaleX As Single
        Get
            Return scaleX_
        End Get
        Set(ByVal value As Single)
            scaleX_ = value
            contentSize_ = New SizeF(originalContentSize_.Width * scaleX_, originalContentSize_.Height * scaleY_)
            center = center_ ' keep center the same, location will automatically changed
        End Set
    End Property
    Public Property scaleY As Single
        Get
            Return scaleY_
        End Get
        Set(ByVal value As Single)
            scaleY_ = value
            contentSize_ = New SizeF(originalContentSize_.Width * scaleX_, originalContentSize_.Height * scaleY_)
            center = center_ ' keep center the same, location will automatically changed
        End Set
    End Property
    Private opacity_ As Byte ' 0 - 255. 0: transparent; 255: opacity
    Public Overridable Property opacity As Byte
        Get
            Return opacity_
        End Get
        Set(ByVal value As Byte)
            opacity_ = value
        End Set
    End Property
    Private rotation_ As Single ' angle of roation in degree 0~360
    Public Overridable Property rotation As Single
        Get
            Return rotation_
        End Get
        Set(ByVal value As Single)
            rotation_ = value
        End Set
    End Property

    Public tag As Integer
    Public parent As CGNode
    Public children As ArrayList
    Public visible As Boolean
    Public userInteractionEnabled As Boolean
    Public Function isTouchForMe(ByVal p As Point) As Boolean
        If Not userInteractionEnabled Then
            Return False
        End If
        Return boundingBox.Contains(p)
    End Function

    Sub New()
        Me.visible = True
        Me.zOrder = 0
        Me.tag = 0
        Me.parent = Nothing
        Me.scale = 1.0
        Me.location = New PointF(0, 0)
        Me.center = New PointF(0, 0)
        Me.contentSize = New SizeF(0, 0)
        Me.rotation = 0
        userInteractionEnabled = False

        actionManager_ = CGDirector.sharedDirector.actionManager
        scheduler_ = CGDirector.sharedDirector.scheduler
        interactionManager_ = CGDirector.sharedDirector.interactionManager
    End Sub

    Public Function boundingBox() As RectangleF
        Return new RectangleF(me.location, me.contentSize)
    End Function

    Public Sub cleanup()
        stopAllActions()
        removeInteraction()
        unscheduleUpdate()

        If children IsNot Nothing Then
            ' clean up self. also clean up all the children of self
            For Each child As CGNode In children
                child.cleanup()
            Next
        End If
    End Sub

#Region "CGNode composition"
    Public Sub addChild(ByVal child As CGNode)
        Debug.Assert(child IsNot Nothing, "child cannot be nil")
        addChild(child, child.zOrder, child.tag)
    End Sub

    Public Sub addChild(ByVal child As CGNode, ByVal z As Integer)
        Debug.Assert(child IsNot Nothing, "child cannot be nil")
        addChild(child, z, child.tag)
    End Sub

    Public Sub addChild(ByVal child As CGNode, ByVal z As Integer, ByVal tag As Integer)
        Debug.Assert(child IsNot Nothing, "child cannot be nil")
        Debug.Assert(child.parent Is Nothing, "child already added. It cannot be added again.")
        If children Is Nothing Then
            children = New ArrayList
        End If

        insertChild(child, z)

        child.tag = tag
        child.parent = Me
    End Sub

    Private Sub reorderChild(ByVal child As CGNode, ByVal z As Integer)
        Debug.Assert(child IsNot Nothing, "child cannot be nil")
        isReorderChildDirty_ = True
        child._setZOrder(z)
    End Sub

    Private Sub insertChild(ByVal child As CGNode, ByVal z As Integer)
        isReorderChildDirty_ = True
        children.Add(child)
        child._setZOrder(z)
    End Sub

    Public Sub sortAllChildren()
        If isReorderChildDirty_ Then
            ' insertion sort. insert tempItem into proper position
            ' [0, i] is always ordered.
            For i As Integer = 1 To children.Count - 1
                Dim j As Integer = i - 1
                Dim tempItem As CGNode = children.Item(i)
                ' continue moving element downwards while zOrder of j-th element is greater than tempItem's. 
                While j >= 0 AndAlso (tempItem.zOrder < CType(children.Item(j), CGNode).zOrder)
                    children.Item(j + 1) = children.Item(j)
                    j = j - 1
                End While
                children.Item(j + 1) = tempItem
            Next

            isReorderChildDirty_ = False
        End If
    End Sub

    Public Sub removeFromParentAndCleanup(ByVal cleanup As Boolean)
        If parent IsNot Nothing Then
            parent.removeChild(Me, cleanup)
        End If
    End Sub

    Public Sub removeChild(ByVal child As CGNode, ByVal cleanup As Boolean)
        If child Is Nothing Then
            Return
        End If

        If children.Contains(child) Then
            detachChild(child, cleanup)
        End If
    End Sub

    Public Sub removeChildByTag(ByVal tag As Integer, ByVal cleanup As Boolean)
        Dim child As CGNode = getChildByTag(tag)
        If child Is Nothing Then
            Debug.WriteLine("removeChildByTag: child not found")
        Else
            removeChild(child, cleanup)
        End If
    End Sub

    Public Sub removeAllChildren(ByVal cleanup As Boolean)
        For Each node As CGNode In children
            If cleanup Then
                node.cleanup()
            End If
            node.parent = Nothing
        Next
        children.Clear()
    End Sub

    Private Sub detachChild(ByVal child As CGNode, ByVal cleanup As Boolean)
        If cleanup Then
            child.cleanup()
        End If
        child.parent = Nothing
        children.Remove(child)
    End Sub

    Private Function getChildByTag(ByVal tag As Integer) As CGNode
        For Each node As CGNode In children
            If node.tag = tag Then
                Return node
            End If
        Next
        Return Nothing
    End Function
#End Region

#Region "Actions"
    Public Function actionManager() As CGActionManager
        Return actionManager_
    End Function

    Public Sub stopAllActions()
        actionManager_.removeAllActionsFromTarget(Me)
    End Sub

    Public Sub stopActionByTag(ByVal tag As Integer)
        actionManager_.removeActionByTag(tag, Me)
    End Sub

    Public Sub stopAction(ByVal action As CGAction)
        actionManager_.removeAction(action)
    End Sub

    Public Sub runAction(ByVal action As CGAction)
        Debug.Assert(action IsNot Nothing, "action cannot be nothing")
        actionManager_.addAction(action, Me, False)
    End Sub

    Public Function getActionByTag(ByVal tag As Integer) As CGAction
        Dim action As CGAction = Nothing
        action = actionManager_.getActionByTag(tag, Me)
        Return action
    End Function

    Public Function numberOfRunningActions() As UInteger
        Return actionManager_.numberOfRunningActionsInTarget(Me)
    End Function
#End Region

#Region "Interactions"
    Public Function interactionManager() As CGInteractionManager
        Return interactionManager_
    End Function

    Public Sub removeInteraction()
        interactionManager_.removeInteractionFromTarget(Me)
    End Sub

    Public Sub addInteraction(ByVal interaction As CGInteraction)
        Debug.Assert(interaction IsNot Nothing, "interaction cannot be nothing")
        interactionManager_.addInteraction(interaction, Me)
    End Sub
#End Region

#Region "Scheduler"
    Public Sub unscheduleUpdate()
        ' TODO
    End Sub

    Public Sub scheduleUpdate()
        ' TODO
    End Sub
#End Region

    Public Overridable Sub draw()
        ' overrides me!
    End Sub

    Public Overridable Sub visit()
        If Not visible Then
            Return
        End If

        If children IsNot Nothing Then
            sortAllChildren()

            ' draw children with zOrder < 0
            For Each child As CGNode In children
                If child.zOrder < 0 Then
                    child.visit()
                Else
                    Exit For
                End If
            Next

            draw() ' draw self

            ' draw children with zOrder >= 0
            For Each child As CGNode In children
                If child.zOrder >= 0 Then
                    child.visit()
                End If
            Next
        Else
            draw()
        End If
    End Sub

    Public Sub _setZOrder(ByVal z As Integer)
        zOrder_ = z
    End Sub
End Class
