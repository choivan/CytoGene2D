Public Class CGDirector
    Private Shared sharedDirector_ As CGDirector
    Private Shared lock_ As New Object

    Private WithEvents mainWindow_ As Form
    Private WithEvents canvas_ As PictureBox
    Private canvasSize_ As Size
    Private canvasBuffer_ As Bitmap
    Private canvasContext_ As Graphics ' front
    Private canvasBufferContext_ As Graphics ' back graphics context
    Public Property canvas As PictureBox
        Get
            Return canvas_
        End Get
        Set(ByVal value As PictureBox)
            ' set canvas when initializing or size has been changed.
            If canvas_ Is Nothing OrElse Not canvasBuffer_.Size.Equals(value.ClientSize) And Not value.ClientSize.Equals(Size.Empty) Then
                canvas_ = value
                canvasSize_ = value.Size
                canvasBuffer_ = New Bitmap(canvasSize_.Width, canvasSize_.Height)
                canvasContext_ = canvas_.CreateGraphics
                canvasBufferContext_ = Graphics.FromImage(canvasBuffer_)
                canvasBufferContext_.SmoothingMode = Drawing.Drawing2D.SmoothingMode.HighQuality
                canvasBufferContext_.InterpolationMode = Drawing.Drawing2D.InterpolationMode.High
            End If
        End Set
    End Property
    Public ReadOnly Property canvasWidth As Single
        Get
            Return canvasSize_.Width
        End Get
    End Property
    Public ReadOnly Property canvasHeight As Single
        Get
            Return canvasSize_.Height
        End Get
    End Property
    Public ReadOnly Property canvasSize As Size
        Get
            Return canvasSize_
        End Get
    End Property
    Public ReadOnly Property canvasBounds As Rectangle
        Get
            Return New Rectangle(0, 0, canvasSize.Width, canvasSize.Height)
        End Get
    End Property
    Public ReadOnly Property graphicsContext As Graphics
        Get
            Return canvasBufferContext_
        End Get
    End Property
    Public Property mainWindow As Form
        Get
            Return mainWindow_
        End Get
        Set(ByVal value As Form)
            mainWindow_ = value
        End Set
    End Property
    Private animationInterval_ As Single '1 = 1 second.
    Public Property animationInterval As Single
        Get
            Return animationInterval_
        End Get
        Set(ByVal value As Single)
            animationInterval_ = value
            If timer_ IsNot Nothing Then
                stopAnimation()
                startAnimation()
            End If
        End Set
    End Property
    'Public isPaused As Boolean
    Public isAnimating As Boolean

    Private currentScene_ As CGScene
    Public ReadOnly Property currentScene As CGScene
        Get
            Return currentScene_
        End Get
    End Property
    Private WithEvents timer_ As Windows.Forms.Timer
    Private actionManager_ As CGActionManager
    Public ReadOnly Property actionManager As CGActionManager
        Get
            Return actionManager_
        End Get
    End Property
    Private scheduler_ As CGScheduler
    Public ReadOnly Property scheduler As CGScheduler
        Get
            Return scheduler_
        End Get
    End Property
    Private interactionManager_ As CGInteractionManager
    Public ReadOnly Property interactionManager As CGInteractionManager
        Get
            Return interactionManager_
        End Get
    End Property

    Protected Sub New()
        currentScene_ = Nothing
        canvas_ = Nothing
        canvasSize_ = New Size(0, 0)
        'isPaused = True
        isAnimating = False
        animationInterval = 0.04

        scheduler_ = New CGScheduler
        actionManager_ = New CGActionManager
        scheduler_.scheduleUpdate(actionManager_, kCGPrioritySystem, False)
        interactionManager_ = New CGInteractionManager ' interaction manager is not scheduled. it is driven by user mouse / keyboard event
    End Sub

    ' generate singleton Director
    Public Shared Function sharedDirector() As CGDirector
        If sharedDirector_ Is Nothing Then ' update sync code to improve performance
            SyncLock lock_
                If sharedDirector_ Is Nothing Then
                    sharedDirector_ = New CGDirector
                End If
            End SyncLock
        End If
        Return sharedDirector_
    End Function

    Public Sub startAnimation()
        If isAnimating Then
            Return
        End If
        isAnimating = True
        timer_ = New Windows.Forms.Timer()
        timer_.Interval = animationInterval * 1000
        timer_.Start()
    End Sub

    Public Sub stopAnimation()
        If Not isAnimating Then
            Return
        End If
        timer_.Stop()
        timer_ = Nothing
        isAnimating = False
    End Sub

    Public Sub runScene(ByVal scene As CGScene)
        currentScene_ = scene
        startAnimation()
    End Sub

    Public Sub drawScene()
        If Not isAnimating Or currentScene_ Is Nothing Then
            Return
        Else
            scheduler_.update()
        End If
        ' clear background to white. 
        '  IS IT REALLY NEEDED TO RESET THE BACKGROUND TO WHITE HERE?
        '    In the future, maybe can provide a colored layer to customize.
        canvasBufferContext_.FillRectangle(Brushes.White, 0, 0, canvasSize_.Width, canvasSize_.Height)

        ' draw content
        currentScene_.visit()

        canvasContext_.DrawImage(canvasBuffer_, New Point(0, 0))
    End Sub

    Private Sub mainLoop(ByVal sender As Object, ByVal e As System.EventArgs) Handles timer_.Tick
        drawScene()
    End Sub

    Private Sub canvas__MouseClick(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles canvas_.MouseClick
        interactionManager_.update(sender, e, MouseEvent.MouseClick)
    End Sub

    Private Sub canvas__MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles canvas_.MouseDown
        interactionManager_.update(sender, e, MouseEvent.MouseDown)
    End Sub

    Private Sub canvas__MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles canvas_.MouseMove
        interactionManager_.update(sender, e, MouseEvent.MouseMove)
    End Sub

    Private Sub canvas__MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles canvas_.MouseUp
        interactionManager_.update(sender, e, MouseEvent.MouseUp)
    End Sub

    'responding to resizing event
    Private Sub canvas__SizeChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles canvas_.SizeChanged
        Me.canvas = sender
    End Sub

    Private Sub mainWindow__Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles mainWindow_.Resize
        If mainWindow_.WindowState = FormWindowState.Minimized Then
            Me.stopAnimation()
        ElseIf mainWindow_.WindowState = FormWindowState.Normal Then
            Me.startAnimation()
        End If
    End Sub

    Private Sub mainWindow__ResizeBegin(ByVal sender As Object, ByVal e As System.EventArgs) Handles mainWindow_.ResizeBegin
        Me.stopAnimation()
    End Sub

    Private Sub mainWindow__ResizeEnd(ByVal sender As Object, ByVal e As System.EventArgs) Handles mainWindow_.ResizeEnd
        Me.startAnimation()
    End Sub
End Class
