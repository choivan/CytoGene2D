Public Class CGSimpleGUILayer : Inherits CGNode
    Private WithEvents mainWindow_ As Form
    Private bottomBar_ As CGSprite ' Self draw bottom bar. do not add it as a child
    Private rightButton_ As CGButtonSprite
    Private leftButton_ As CGButtonSprite

    Public ReadOnly Property rightButton As CGButtonSprite
        Get
            Return rightButton_
        End Get
    End Property

    Public ReadOnly Property leftButton As CGButtonSprite
        Get
            Return leftButton_
        End Get
    End Property

    Sub New()
        mainWindow_ = CGDirector.sharedDirector.mainWindow

        location = PointF.Empty
        bottomBar_ = New CGSprite(My.Resources.SimpleGUIBottomBar)
        rightButton_ = New CGButtonSprite(PointF.Empty,
                                          My.Resources.simple_right_normal,
                                          My.Resources.simple_right_selected,
                                          My.Resources.simple_right_highlighted,
                                          My.Resources.simple_right_disabled)
        leftButton_ = New CGButtonSprite(PointF.Empty,
                                         My.Resources.simple_left_normal,
                                         My.Resources.simple_left_selected,
                                         My.Resources.simple_left_highlighted,
                                         My.Resources.simple_left_disabled)
        Me.addChild(rightButton_)
        Me.addChild(leftButton_)
        resetUIElementsPosition()
    End Sub

    Public Sub setClickHandlerOfButton(ByVal button As CGButtonSprite, ByVal Clickhandler As CGButtonBase.MouseEventHandler)
        button.clickHandler = Clickhandler
    End Sub

    Public Overrides Sub draw()
        MyBase.draw()
        Dim context As Graphics = CGDirector.sharedDirector.graphicsContext

        ''''' Draw the bottom bar
        Dim originInterpolationMode As Drawing.Drawing2D.InterpolationMode = context.InterpolationMode
        Dim originPixelOffsetMode As Drawing.Drawing2D.PixelOffsetMode = context.PixelOffsetMode
        context.InterpolationMode = Drawing2D.InterpolationMode.NearestNeighbor
        context.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
        context.DrawImage(bottomBar_.texture, Rectangle.Round(bottomBar_.boundingBox))
        context.InterpolationMode = originInterpolationMode
        context.PixelOffsetMode = originPixelOffsetMode
        '''''
    End Sub

    Private Sub resetUIElementsPosition()
        contentSize = CGDirector.sharedDirector.canvasSize
        bottomBar_.contentSize = New SizeF(contentSize.Width, bottomBar_.contentSize.Height)
        bottomBar_.location = New PointF(0, contentSize.Height - bottomBar_.contentSize.Height)
        rightButton_.center = New PointF(contentSize.Width / 2 + 1.5 * rightButton_.contentSize.Width,
                                         contentSize.Height - rightButton_.contentSize.Height / 2)
        leftButton_.center = New PointF(contentSize.Width / 2 - 1.5 * leftButton_.contentSize.Width,
                                        contentSize.Height - leftButton_.contentSize.Height / 2)
    End Sub

    Private Sub mainWindow__SizeChanged(sender As Object, e As EventArgs) Handles mainWindow_.SizeChanged
        resetUIElementsPosition()
    End Sub
End Class
