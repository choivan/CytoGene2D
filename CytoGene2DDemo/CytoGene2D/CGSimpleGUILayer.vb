Imports System.Drawing
Imports System.Drawing.Printing
Public Class CGSimpleGUILayer : Inherits CGNode
    Private WithEvents mainWindow_ As Form
    Private bottomBar_ As CGSprite ' Self draw bottom bar. do not add it as a child
    Private rightButton_ As CGButtonSprite
    Private leftButton_ As CGButtonSprite
    Private miniButton_ As CGButtonSprite
    Private closeButton_ As CGButtonSprite
    Private printButton_ As CGButtonSprite
    Private playButton_ As CGButtonToggleSprite
    Private isFullScreen_ As Boolean

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

    Public ReadOnly Property BottomBarHeight As Single
        Get
            Return bottomBar_.boundingBox.Height
        End Get
    End Property

    Sub New(Optional isFullScreen As Boolean = False)
        isFullScreen_ = isFullScreen
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
        printButton_ = New CGButtonSprite(PointF.Empty,
                                          My.Resources.simple_print_normal,
                                          My.Resources.simple_print_selected,
                                          My.Resources.simple_print_highlighted,
                                          My.Resources.simple_print_disabled)
        playButton_ = New CGButtonToggleSprite(PointF.Empty,
                                               My.Resources.simple_play_on,
                                               My.Resources.simple_play_off,
                                               My.Resources.simple_play_on_selected,
                                               My.Resources.simple_play_off_selected)
        Me.addChild(playButton_)
        Me.addChild(rightButton_)
        Me.addChild(leftButton_)
        Me.addChild(printButton_)
        playButton_.toggleHandler = AddressOf toggle
        printButton_.clickHandler = AddressOf print
        If isFullScreen Then
            miniButton_ = New CGButtonSprite(PointF.Empty,
                                         My.Resources.simple_mini_normal,
                                         My.Resources.simple_mini_selected)
            closeButton_ = New CGButtonSprite(PointF.Empty,
                                              My.Resources.simple_close_normal,
                                              My.Resources.simple_close_selected)
            Dim miniInteraction As New CGInteractionButton
            miniButton_.addInteraction(miniInteraction)
            Dim closeInteraction As New CGInteractionButton
            closeButton_.addInteraction(closeInteraction)
            Me.addChild(miniButton_)
            Me.addChild(closeButton_)
            miniButton_.clickHandler = Sub(sender As CGButtonBase, e As MouseEventArgs, info As Object)
                                           Debug.Assert(CGDirector.sharedDirector.mainWindow IsNot Nothing, Me.ToString + ": Main window property is not set up")
                                           CGDirector.sharedDirector.mainWindow.WindowState = FormWindowState.Minimized
                                       End Sub
            closeButton_.clickHandler = Sub(sender As CGButtonBase, e As MouseEventArgs, info As Object)
                                            Debug.Assert(CGDirector.sharedDirector.mainWindow IsNot Nothing, Me.ToString + ": Main window property is not set up")
                                            CGDirector.sharedDirector.mainWindow.Close()
                                        End Sub
        End If
        resetUIElementsPosition()
    End Sub

    Public Sub setClickHandlerOfButton(ByVal button As CGButtonSprite, ByVal Clickhandler As CGButtonBase.MouseEventHandler)
        button.clickHandler = Clickhandler
    End Sub

    Public Sub highlightButtonDelayed(button As CGButtonSprite, delayTime As Integer)
        Me.runAction(CGSequence.actionWithArray({New CGDelayTime(delayTime),
                                                 New CGActionInstant(Sub() button.setHighlighted())}))
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

    Private Sub toggle(sender As CGButtonToggle, e As MouseEventArgs, info As Object)
        If CGDirector.sharedDirector.isAnimating Then
            CGDirector.sharedDirector.stopAnimation()
        Else
            CGDirector.sharedDirector.startAnimation()
        End If
    End Sub

    Private Sub print(sender As CGButtonBase, e As MouseEventArgs, info As Object)
        Dim pd As New Printing.PrintDocument
        pd.DefaultPageSettings.Landscape = True
        pd.DefaultPageSettings.PaperSize = New PaperSize("A4", 595, 842)
        AddHandler pd.PrintPage, AddressOf Me.pd_PrintPage
        pd.Print()
    End Sub

    Private Sub pd_PrintPage(ByVal sender As Object, ByVal ev As Printing.PrintPageEventArgs)
        Dim marginLeft As Integer = 0
        Dim marginTop As Integer = ev.MarginBounds.Top - 20
        ev.Graphics.DrawImage(CGDirector.sharedDirector.canvasBuff, marginLeft, marginTop)
    End Sub

    Private Sub resetUIElementsPosition()
        contentSize = CGDirector.sharedDirector.canvasSize
        bottomBar_.contentSize = New SizeF(contentSize.Width, bottomBar_.contentSize.Height)
        bottomBar_.location = New PointF(0, contentSize.Height - bottomBar_.contentSize.Height)
        rightButton_.center = New PointF(contentSize.Width / 2 + 1.5 * rightButton_.contentSize.Width,
                                         contentSize.Height - rightButton_.contentSize.Height / 2)
        leftButton_.center = New PointF(contentSize.Width / 2 - 1.5 * leftButton_.contentSize.Width,
                                        contentSize.Height - leftButton_.contentSize.Height / 2)
        printButton_.center = New PointF(contentSize.Width - 2 * printButton_.contentSize.Width,
                                         contentSize.Height - printButton_.contentSize.Height / 2)
        playButton_.center = New PointF(2 * playButton_.contentSize.Width,
                                        contentSize.Height - playButton_.contentSize.Height / 2)
        If isFullScreen_ Then
            miniButton_.center = New PointF(contentSize.Width - miniButton_.contentSize.Width * 1.75, miniButton_.contentSize.Height * 0.75)
            closeButton_.center = New PointF(contentSize.Width - closeButton_.contentSize.Width * 0.75, closeButton_.contentSize.Height * 0.75)
        End If
    End Sub

    Private Sub mainWindow__SizeChanged(sender As Object, e As EventArgs) Handles mainWindow_.SizeChanged
        resetUIElementsPosition()
    End Sub
End Class
