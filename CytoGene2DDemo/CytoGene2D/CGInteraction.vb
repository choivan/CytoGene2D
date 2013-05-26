Public Class CGInteraction
    Delegate Sub InteractionHandler(sender As Object, info As Object)
    Public completionHandler As InteractionHandler = Nothing ' when the interaction is done, the completion handler is called to inform

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

    Public swallowMouseEvent As Boolean

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
        ' 2013/4/30 add "status <> InteractionStatus.MouseMove" to fix dragging lost when mouse moving fast
        swallowMouseEvent = False ' not tested. Be aware of this potential issue when interaction is acting strong.
        If target_.isTouchForMe(e.Location) OrElse status = InteractionStatus.MouseMove OrElse status = InteractionStatus.MouseDown Then
            If status_ = InteractionStatus.MouseIdle Then
                If m = MouseEvent.MouseDown Then
                    status_ = InteractionStatus.MouseDown
                    swallowMouseEvent = True
                End If
            ElseIf status_ = InteractionStatus.MouseDown Then
                If m = MouseEvent.MouseMove Then
                    status_ = InteractionStatus.MouseMove
                    swallowMouseEvent = True
                ElseIf m = MouseEvent.MouseUp Then
                    status_ = InteractionStatus.MouseIdle
                    swallowMouseEvent = True
                End If
            ElseIf status_ = InteractionStatus.MouseMove Then
                If m = MouseEvent.MouseUp Then
                    status_ = InteractionStatus.MouseIdle
                    swallowMouseEvent = True
                End If
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
                target.runAction(New CGSequence(moveTo,
                                                New CGActionInstant(Sub()
                                                                        isDone_ = True
                                                                        If completionHandler IsNot Nothing Then
                                                                            completionHandler.Invoke(target, Nothing)
                                                                        End If
                                                                    End Sub)))
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
    Public onlyAvailableHighlighted As Boolean = False
    Private acceptClick_ As Boolean = False
    Public Overrides Sub startWithTarget(ByVal target As Object)
        Debug.Assert(target.GetType() Is GetType(CGButtonBase) OrElse
                     target.GetType().IsSubclassOf(GetType(CGButtonBase)),
                     Me.ToString + ": Invalid type")
        MyBase.startWithTarget(target)
    End Sub

    Public Overrides Sub update(sender As Object, e As MouseEventArgs, m As MouseEvent)
        swallowMouseEvent = False
        If target.status = ButtonStatus.ButtonDisabled Then
            status = InteractionStatus.MouseIdle
            Return
        End If
        If Not target.isTouchForMe(e.Location) Then ' button rarely moves, if mouse is out of the button, then just set the button state back to normal
            status = InteractionStatus.MouseIdle
            If target.status <> ButtonStatus.ButtonHighlighted Then target.setNormal()
            If acceptClick_ AndAlso onlyAvailableHighlighted Then target.setHighlighted()
            acceptClick_ = False
            Return
        End If
        MyBase.update(sender, e, m)
        If status = InteractionStatus.MouseIdle Then
            If m = MouseEvent.MouseMove Then
                'target.setHighlighted()
            ElseIf m = MouseEvent.MouseUp Then
                If acceptClick_ OrElse
                    (onlyAvailableHighlighted And target.status = ButtonStatus.ButtonHighlighted) Then ' fix lost click event when onlyavailablewhenhighlighted enabled
                    acceptClick_ = False
                    target.click(e)
                End If
                target.setNormal()
                'If Not onlyAvailableHighlighted OrElse
                '    (onlyAvailableHighlighted AndAlso target.status = ButtonStatus.ButtonHighlighted) Then
                '    target.click(e)
                'End If
                'If target.status <> ButtonStatus.ButtonHighlighted Then target.setNormal()
            End If
        ElseIf status = InteractionStatus.MouseDown Then
            Dim lastStatus As ButtonStatus = target.status
            If Not onlyAvailableHighlighted OrElse
                    (onlyAvailableHighlighted AndAlso target.status = ButtonStatus.ButtonHighlighted) Then
                acceptClick_ = True
            End If
            target.setSelected()
            'If lastStatus = ButtonStatus.ButtonHighlighted Then
            '    target.setHighlighted()
            'End If
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
        swallowMouseEvent = False
        If target.status = CGConstant.ButtonStatus.ButtonDisabled Then
            status = InteractionStatus.MouseIdle
            Return
        End If
        If Not target.isTouchForMe(e.Location) Then
            status = InteractionStatus.MouseIdle
            If target.status <> ButtonStatus.ButtonHighlighted Then target.setNormal()
            Return
        End If
        MyBase.update(sender, e, m)
        If m = MouseEvent.MouseUp Then
            target.toggle(e, Nothing)
            If target.status <> ButtonStatus.ButtonHighlighted Then target.setNormal()
        ElseIf m = MouseEvent.MouseDown Then
            target.setSelected()
        End If
    End Sub
End Class