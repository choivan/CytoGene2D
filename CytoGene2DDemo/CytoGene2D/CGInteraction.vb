Public Class CGInteraction

    Private status_ As InteractionStatus
    Public Property status As InteractionStatus
        Get
            Return status_
        End Get
        Set(ByVal value As InteractionStatus)
            status_ = value
        End Set
    End Property
    Private target_ As Object
    Public Property target As Object
        Get
            Return target_
        End Get
        Set(ByVal value As Object)
            target_ = value
        End Set
    End Property

    Public Overridable Function didTriggerHotSpot() As Boolean
        ' overrides me
        Return False
    End Function

    Public Overridable Function isDone() As Boolean
        ' overrides me
        Return False
    End Function

    Public Overridable Sub stopInteraction()
        ' overrides me
        target_.userInteractionEnabled = False
        target_ = Nothing
    End Sub

    Public Overridable Sub update(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs, ByVal m As MouseEvent)
        ' overrides me
        If Not target_.isTouchForMe(e.Location) Then Return
        If status_ = InteractionStatus.MouseIdle Then
            If m = MouseEvent.MouseDown Then
                status_ = InteractionStatus.MouseDown
            End If
        ElseIf status_ = InteractionStatus.MouseDown Then
            If m = MouseEvent.MouseMove Then
                status_ = InteractionStatus.MouseMove
            ElseIf m = MouseEvent.MouseUp Then
                status_ = InteractionStatus.MouseIdle
            End If
        ElseIf status_ = InteractionStatus.MouseMove Then
            If m = MouseEvent.MouseUp Then
                status_ = InteractionStatus.MouseIdle
            End If
        End If
    End Sub

    Public Overridable Sub startWithTarget(ByVal target As Object)
        target_ = target
        target_.userInteractionEnabled = True
        status_ = InteractionStatus.MouseIdle
    End Sub

End Class

Public Class CGInteractionMoveTo : Inherits CGInteraction
    Private destination_ As PointF
    Public Property destination As PointF
        Get
            Return destination
        End Get
        Set(ByVal value As PointF)
            destination_ = value
        End Set
    End Property
    Private destRect_ As RectangleF
    Private original_ As PointF
    Private lastMouseLocation_ As Point
    Private isDone_ As Boolean

    Public hasIndicator As Boolean
    Private isIndicatorAdded_ As Boolean
    Private indicator_ As CGIndicator

    Sub New(ByVal destination As PointF, ByVal hasIndicator As Boolean)
        destination_ = destination
        isDone_ = False
        Me.hasIndicator = hasIndicator
    End Sub

    Public Overrides Sub startWithTarget(ByVal target As Object)
        MyBase.startWithTarget(target)
        original_ = target.center
        destRect_ = New RectangleF(destination_.X - target.contentSize.Width / 2, destination_.Y - target.contentSize.Height / 2,
                                   target.contentSize.Width, target.contentSize.Height)
    End Sub

    Public Overrides Function didTriggerHotSpot() As Boolean
        Return destRect_.Contains(target.center)
    End Function

    Public Overrides Function isDone() As Boolean
        Return isDone_
    End Function

    Public Overrides Sub update(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs, ByVal m As MouseEvent)
        If Not target.isTouchForMe(e.Location) Then Return
        MyBase.update(sender, e, m)
        If status = InteractionStatus.MouseIdle AndAlso m = MouseEvent.MouseUp Then
            If hasIndicator AndAlso isIndicatorAdded_ Then
                indicator_.stopAllActions()
                indicator_.removeFromParentAndCleanup(True)
                indicator_ = Nothing
                isIndicatorAdded_ = False
            End If
            If didTriggerHotSpot() Then
                Dim moveTo As New CGMoveTo(3, destination_)
                target.runAction(moveTo)
                isDone_ = True
            Else
                ' cool!! animation!!!
                target.userInteractionEnabled = False
                Dim moveTo As New CGMoveTo(5, original_)
                Dim callBack As New CGActionInstant(Sub()
                                                        target.userInteractionEnabled = True
                                                    End Sub)
                target.runAction(New CGSequence(moveTo, callBack))
            End If
        ElseIf status = InteractionStatus.MouseMove AndAlso m = MouseEvent.MouseMove Then
            target.center = New PointF(target.center.X + e.X - lastMouseLocation_.X, target.center.Y + e.Y - lastMouseLocation_.Y)
            lastMouseLocation_ = e.Location
        ElseIf status = InteractionStatus.MouseDown AndAlso m = MouseEvent.MouseDown AndAlso Not isIndicatorAdded_ Then
            If hasIndicator Then
                indicator_ = New CGIndicator(destRect_, Color.Red)
                CGDirector.sharedDirector.currentScene.addChild(indicator_, kCGTopMostZOrder)
                Dim foreverBlink As New CGInfiniteTimeAction(New CGBlink(15, 1))
                indicator_.runAction(foreverBlink)
                isIndicatorAdded_ = True
            End If
        End If
        If m = MouseEvent.MouseDown Then
            lastMouseLocation_ = e.Location
        End If
    End Sub

End Class

Public Class CGInteractionMoveBy : Inherits CGInteractionMoveTo
    Private delta_ As PointF

    Sub New(ByVal delta As PointF, ByVal hasIndicator As Boolean)
        MyBase.New(Nothing, hasIndicator)
        delta_ = delta
    End Sub

    Public Overrides Sub startWithTarget(ByVal target As Object)
        destination = New PointF(target.center.X + delta_.X, target.center.Y + delta_.Y)
        MyBase.startWithTarget(target)
    End Sub
End Class

' NOTE: the interactions with 'Button' in its name only works for CGButtonBase (or subclass of it) object, otherwise, error throws.
Public Class CGInteractionButton : Inherits CGInteraction
    Public Overrides Sub startWithTarget(ByVal target As Object)
        Debug.Assert(target.GetType() Is GetType(CGButtonBase) OrElse
                     target.GetType().IsSubclassOf(GetType(CGButtonBase)),
                     Me.ToString + ": Invalid type")
        MyBase.startWithTarget(target)
    End Sub

    Public Overrides Sub update(sender As Object, e As MouseEventArgs, m As MouseEvent)
        If target.status = CGConstant.ButtonStatus.ButtonDisabled Then Return

        If Not target.isTouchForMe(e.Location) Then
            status = InteractionStatus.MouseIdle
            target.setNormal()
            Return
        End If
        MyBase.update(sender, e, m)
        If status = InteractionStatus.MouseIdle Then
            If m = MouseEvent.MouseMove Then
                'target.setHighlighted()
            ElseIf m = MouseEvent.MouseUp Then
                target.setNormal()
            End If
        ElseIf status = InteractionStatus.MouseDown OrElse status = InteractionStatus.MouseMove Then
            target.setSelected()
            If m = MouseEvent.MouseClick Then
                target.click(e)
            End If
        End If
    End Sub
End Class

Public Class CGInteractionButtonToggle : Inherits CGInteraction
    Public Overrides Sub startWithTarget(ByVal target As Object)
        Debug.Assert(target.GetType() Is GetType(CGButtonBase) OrElse
                     target.GetType().IsSubclassOf(GetType(CGButtonBase)),
                     Me.ToString + ": Invalid type")
        MyBase.startWithTarget(target)
    End Sub

    Public Overrides Sub update(sender As Object, e As MouseEventArgs, m As MouseEvent)
        If target.status = CGConstant.ButtonStatus.ButtonDisabled Then Return
        If Not target.isTouchForMe(e.Location) Then
            status = InteractionStatus.MouseIdle
            target.setNormal()
            Return
        End If
        MyBase.update(sender, e, m)
        If m = MouseEvent.MouseClick Then
            target.toggle(e, Nothing)
        ElseIf m = MouseEvent.MouseUp Then
            target.setNormal()
        ElseIf m = MouseEvent.MouseDown Then
            target.setSelected()
        End If
    End Sub
End Class