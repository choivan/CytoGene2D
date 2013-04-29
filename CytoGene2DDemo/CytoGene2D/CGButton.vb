Public Class CGButton : Inherits CGNode
    ' button properties
    Private status_ As CGConstant.ButtonStatus
    Private normalColor_ As Color
    Private selectedColor_ As Color
    Private highlightedColor_ As Color
    Private disabledColor_ As Color
    Private title_ As String
    Public Property title As String
        Get
            Return title_
        End Get
        Set(value As String)
            title_ = value
        End Set
    End Property
    Private titleFormat_ As StringFormat ' you may consider to expose this property
    Public titleFont As Font

    Private interaction_ As CGInteractionButton

    ' button events
    Event onClick(ByVal sender As CGButton, ByVal e As MouseEventArgs)

    Sub New(ByVal frame As RectangleF)
        title_ = "Button"
        normalColor_ = Color.GhostWhite
        selectedColor_ = Color.Gray
        highlightedColor_ = Color.LightPink
        disabledColor_ = Color.LightGray
        contentSize = frame.Size
        location = frame.Location
        status_ = ButtonStatus.ButtonNormal
        titleFormat_ = New StringFormat
        titleFormat_.Alignment = StringAlignment.Center
        titleFormat_.LineAlignment = StringAlignment.Center
        titleFont = SystemFonts.MessageBoxFont
        interaction_ = New CGInteractionButton
        Me.addInteraction(interaction_)
    End Sub

    Public Overridable Sub setHighlighted()
        status_ = ButtonStatus.ButtonHighlighted
    End Sub

    Public Overridable Sub setDisabled()
        status_ = ButtonStatus.ButtonDisabled
    End Sub

    Public Overridable Sub setNormal()
        status_ = ButtonStatus.ButtonNormal
    End Sub

    Public Overridable Sub setSelected()
        status_ = ButtonStatus.ButtonSelected
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

    Public Sub click(ByVal e As MouseEventArgs)
        RaiseEvent onClick(Me, e)
    End Sub

    Public Overrides Sub draw()
        MyBase.draw()
        Dim context As Graphics = CGDirector.sharedDirector.graphicsContext
        Select Case status_
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
        context.DrawString(title_, titleFont, New SolidBrush(Color.Black), boundingBox, titleFormat_)
    End Sub

End Class

'Public Class CGButtonSprite : Inherits CGButton
'    Private normalSprite_ As CGSprite
'    Private selectedSprite_ As CGSprite
'    Private highlightedSprite_ As CGSprite
'End Class

'Public Class CGButtonToggle : Inherits CGButton

'End Class
