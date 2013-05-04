Public Class CGButtonBase : Inherits CGNode
    Private status_ As CGConstant.ButtonStatus
    Public ReadOnly Property status As CGConstant.ButtonStatus
        Get
            Return status_
        End Get
    End Property
    Public Sub setHighlighted()
        If status_ = ButtonStatus.ButtonDisabled Then Return
        status_ = ButtonStatus.ButtonHighlighted
    End Sub

    Public Sub setEnabled()
        status_ = ButtonStatus.ButtonNormal
    End Sub

    Public Sub setDisabled()
        If status_ = ButtonStatus.ButtonDisabled Then Return
        status_ = ButtonStatus.ButtonDisabled
    End Sub

    Public Sub setNormal()
        If status_ = ButtonStatus.ButtonDisabled Then Return
        status_ = ButtonStatus.ButtonNormal
    End Sub

    Public Sub setSelected()
        If status_ = ButtonStatus.ButtonDisabled Then Return
        status_ = ButtonStatus.ButtonSelected
    End Sub

    Private interaction_ As CGInteraction
    Public WriteOnly Property interaction As CGInteraction ' Buttonbase do not specify which button behavior it is. Set the button behavior on the fly!
        Set(value As CGInteraction)
            Me.removeInteraction()
            interaction_ = value
            Me.addInteraction(interaction_)
        End Set
    End Property
    ' button events (add more if needed)
    ' 1. event-based implementation
    Event onClick(ByVal sender As CGButtonBase, ByVal e As MouseEventArgs)
    Event onToggle(ByVal sender As CGButtonBase, ByVal e As MouseEventArgs, ByVal isOn As Boolean)

    ' 2. Delegate-based implementation (NOTE: This implementation has higher priority, which means if a delegate is set, than the event is never raised)
    '       Programmer is responsible for knowing the type of info
    Delegate Sub MouseEventHandler(ByVal sender As CGButtonBase, ByVal e As MouseEventArgs, ByVal info As Object)
    Public clickHandler As MouseEventHandler = Nothing ' init to nothing
    Public toggleHandler As MouseEventHandler = Nothing

#Region "Mouse Events Handlers"
    Public Overridable Sub click(ByVal e As MouseEventArgs)
        If clickHandler Is Nothing Then
            RaiseEvent onClick(Me, e)
        Else
            clickHandler.Invoke(Me, e, Nothing)
        End If
    End Sub

    Public Overridable Sub toggle(ByVal e As MouseEventArgs, ByVal isOn As Boolean)
        If toggleHandler Is Nothing Then
            RaiseEvent onToggle(Me, e, isOn)
        Else
            toggleHandler.Invoke(Me, e, isOn)
        End If
    End Sub
#End Region

    Sub New()
        status_ = ButtonStatus.ButtonNormal
    End Sub
End Class

Public Class CGButton : Inherits CGButtonBase
    ' button properties
    Private normalColor_ As Color
    Private selectedColor_ As Color
    Private highlightedColor_ As Color
    Private disabledColor_ As Color
    Public title As String
    Public titleFormat As StringFormat
    Public titleFont As Font

    Sub New()
        interaction = New CGInteractionButton
    End Sub

    Sub New(ByVal frame As RectangleF)
        interaction = New CGInteractionButton
        title = "Button"
        normalColor_ = Color.GhostWhite
        selectedColor_ = Color.Gray
        highlightedColor_ = Color.LightPink
        disabledColor_ = Color.LightGray
        boundingBox = frame
        titleFormat = New StringFormat
        titleFormat.Alignment = StringAlignment.Center
        titleFormat.LineAlignment = StringAlignment.Center
        titleFont = SystemFonts.MessageBoxFont
    End Sub

    Protected Sub setNormalColor(ByVal color As Color)
        normalColor_ = color
    End Sub

    Protected Sub setSelectedColor(ByVal color As Color)
        selectedColor_ = color
    End Sub

    Protected Sub setHighlightedColor(ByVal color As Color)
        highlightedColor_ = color
    End Sub

    Protected Sub setDisabledColor(ByVal color As Color)
        disabledColor_ = color
    End Sub

    Public Overrides Sub draw()
        Dim context As Graphics = CGDirector.sharedDirector.graphicsContext
        Select Case status
            Case ButtonStatus.ButtonNormal
                context.FillRectangle(New SolidBrush(normalColor_), boundingBox)
            Case ButtonStatus.ButtonDisabled
                context.FillRectangle(New SolidBrush(disabledColor_), boundingBox)
            Case ButtonStatus.ButtonSelected
                context.FillRectangle(New SolidBrush(selectedColor_), boundingBox)
            Case ButtonStatus.ButtonHighlighted
                context.FillRectangle(New SolidBrush(highlightedColor_), boundingBox)
        End Select
        context.DrawRectangle(Pens.Black, Rectangle.Round(boundingBox))
        context.DrawString(title, titleFont, New SolidBrush(Color.Black), boundingBox, titleFormat)
    End Sub

End Class

Public Class CGButtonSprite : Inherits CGButton
    Public normalImage As Bitmap
    Public selectedImage As Bitmap
    Public highlightedImage As Bitmap
    Public disabledImage As Bitmap

    Public Sub setButtonImages(ByVal normal As Bitmap,
                               ByVal selected As Bitmap,
                               ByVal highlighted As Bitmap,
                               ByVal disabled As Bitmap)
        Debug.Assert(normal.Size = selected.Size And
             normal.Size = highlighted.Size And
             normal.Size = disabled.Size, Me.ToString + ": sprites size not match")
        normalImage = normal
        selectedImage = selected
        highlightedImage = highlighted
        disabledImage = disabled
        contentSize = normal.Size
    End Sub

    Sub New(ByVal normal As Bitmap,
            ByVal selected As Bitmap)
        MyBase.New()
        setButtonImages(normal, selected, selected, normal)
        location = PointF.Empty
    End Sub

    Sub New(ByVal center As PointF,
            ByVal normal As Bitmap,
            ByVal selected As Bitmap)
        MyBase.New()
        setButtonImages(normal, selected, selected, normal)
        Me.center = center
    End Sub

    Sub New(ByVal center As PointF,
            ByVal normal As Bitmap,
            ByVal selected As Bitmap,
            ByVal highlighted As Bitmap,
            ByVal disabled As Bitmap)
        MyBase.New()
        setButtonImages(normal, selected, highlighted, disabled)
        Me.center = center
    End Sub

    Public Overrides Sub draw()
        Dim context As Graphics = CGDirector.sharedDirector.graphicsContext
        Select Case status
            Case ButtonStatus.ButtonNormal
                context.DrawImage(normalImage, boundingBox)
            Case ButtonStatus.ButtonDisabled
                context.DrawImage(disabledImage, boundingBox)
            Case ButtonStatus.ButtonSelected
                context.DrawImage(selectedImage, boundingBox)
            Case ButtonStatus.ButtonHighlighted
                context.DrawImage(highlightedImage, boundingBox)
        End Select
        context.DrawString(title, titleFont, New SolidBrush(Color.Black), boundingBox, titleFormat)
    End Sub
End Class

' Special Button with only ON and OFF state
Public Class CGButtonToggle : Inherits CGButtonBase
    Private isToggleOn_ As Boolean = True
    Public ReadOnly Property isToggleOn As Boolean
        Get
            Return isToggleOn_
        End Get
    End Property
    Private onColor_ As Color
    Public WriteOnly Property onColor As Color
        Set(value As Color)
            onColor_ = value
        End Set
    End Property
    Private offColor_ As Color
    Public WriteOnly Property offColor As Color
        Set(value As Color)
            offColor_ = value
        End Set
    End Property
    Private selectedColor_ As Color
    Public WriteOnly Property selectedColor As Color
        Set(value As Color)
            selectedColor_ = value
        End Set
    End Property
    Private title_ As String
    Public ReadOnly Property title As String
        Get
            Return title_
        End Get
    End Property
    Public onTitle As String
    Public offTitle As String
    Public titleFormat As StringFormat
    Public titleFont As Font

    Sub New()
        interaction = New CGInteractionButtonToggle
    End Sub

    Sub New(ByVal frame As RectangleF)
        interaction = New CGInteractionButtonToggle
        boundingBox = frame
        titleFont = SystemFonts.MessageBoxFont
        titleFormat = New StringFormat
        titleFormat.Alignment = StringAlignment.Center
        titleFormat.LineAlignment = StringAlignment.Center
        setONOFFColorTitle(Color.Gray,
                           Color.GhostWhite, "ON",
                           Color.GhostWhite, "OFF")
    End Sub

    Sub New(ByVal frame As RectangleF, ByVal selectedColor As Color,
            ByVal onColor As Color, ByVal onTitle As String,
            ByVal offColor As Color, ByVal offTitle As String)
        interaction = New CGInteractionButtonToggle
        boundingBox = frame
        titleFont = SystemFonts.MessageBoxFont
        titleFormat = New StringFormat
        titleFormat.Alignment = StringAlignment.Center
        titleFormat.LineAlignment = StringAlignment.Center
        setONOFFColorTitle(selectedColor,
                           onColor, onTitle,
                           offColor, offTitle)
    End Sub

    Public Sub setONOFFColorTitle(ByVal selectedColor As Color,
            ByVal onColor As Color, ByVal onTitle As String,
            ByVal offColor As Color, ByVal offTitle As String)
        selectedColor_ = selectedColor
        onColor_ = onColor
        Me.onTitle = onTitle
        offColor_ = offColor
        Me.offTitle = offTitle
    End Sub

    Public Overrides Sub toggle(e As MouseEventArgs, isOn As Boolean)
        isToggleOn_ = Not isToggleOn_
        MyBase.toggle(e, isToggleOn_)
    End Sub

    Public Overrides Sub draw()
        Dim context As Graphics = CGDirector.sharedDirector.graphicsContext
        If status = ButtonStatus.ButtonNormal Then
            If isToggleOn_ Then
                title_ = onTitle
                context.FillRectangle(New SolidBrush(onColor_), Rectangle.Round(boundingBox))
            Else
                title_ = offTitle
                context.FillRectangle(New SolidBrush(offColor_), Rectangle.Round(boundingBox))
            End If
        ElseIf status = ButtonStatus.ButtonSelected Then
            context.FillRectangle(New SolidBrush(selectedColor_), Rectangle.Round(boundingBox))
        End If
        context.DrawRectangle(Pens.Black, Rectangle.Round(boundingBox))
        context.DrawString(title, titleFont, New SolidBrush(Color.Black), boundingBox, titleFormat)
    End Sub
End Class

Public Class CGButtonToggleSprite : Inherits CGButtonToggle
    Public onImage As Bitmap
    Public offImage As Bitmap
    Public onSelectedImage As Bitmap
    Public offSelectedImage As Bitmap

    Public Sub setButtonImages(ByVal onImage As Bitmap,
                               ByVal offImage As Bitmap,
                               ByVal onSelectedImage As Bitmap,
                               ByVal offSelectedImage As Bitmap)
        Debug.Assert(onImage.Size = onSelectedImage.Size And
                     onImage.Size = offImage.Size, Me.ToString + ": sprites size not match")
        Me.onImage = onImage
        Me.onSelectedImage = onSelectedImage
        Me.offSelectedImage = offSelectedImage
        Me.offImage = offImage
        contentSize = onImage.Size
    End Sub

    Sub New(ByVal onImage As Bitmap,
            ByVal offImage As Bitmap)
        MyBase.New()
        Debug.Assert(onImage.Size = offImage.Size, Me.ToString + ": image size not match")
        setButtonImages(onImage, offImage, onImage, offImage)
        location = PointF.Empty
    End Sub

    Sub New(ByVal center As PointF,
            ByVal onImage As Bitmap,
            ByVal offImage As Bitmap)
        MyBase.New()
        setButtonImages(onImage, offImage, onImage, offImage)
        Me.center = center
    End Sub

    Sub New(ByVal center As PointF,
            ByVal onImage As Bitmap,
            ByVal offImage As Bitmap,
            ByVal onSelectedImage As Bitmap,
            ByVal offSelectedImage As Bitmap)
        MyBase.New()
        setButtonImages(onImage, offImage, onSelectedImage, offSelectedImage)
        Me.center = center
    End Sub

    Public Overrides Sub draw()
        Dim context As Graphics = CGDirector.sharedDirector.graphicsContext
        If status = ButtonStatus.ButtonNormal Then
            If isToggleOn Then
                context.DrawImage(onImage, boundingBox)
            Else
                context.DrawImage(offImage, boundingBox)
            End If
        ElseIf status = ButtonStatus.ButtonSelected Then
            If isToggleOn Then
                context.DrawImage(onSelectedImage, boundingBox)
            Else
                context.DrawImage(offSelectedImage, boundingBox)
            End If
        End If
    End Sub
End Class
