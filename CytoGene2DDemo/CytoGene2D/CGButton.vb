Public Class CGButton : Inherits CGNode
    Private status_ As CGConstant.ButtonStatus

    Sub New(ByVal frame As RectangleF)
        contentSize = frame.Size
        location = frame.Location
        status_ = ButtonStatus.ButtonNormal
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

    Public Overrides Sub draw()
        MyBase.draw()
        Dim context As Graphics = CGDirector.sharedDirector.graphicsContext
        Select Case status_
            Case ButtonStatus.ButtonNormal
                context.FillRectangle(New SolidBrush(Color.White), boundingBox)
            Case ButtonStatus.ButtonDisabled
                context.FillRectangle(New SolidBrush(Color.LightGray), boundingBox)
            Case ButtonStatus.ButtonSelected
                context.FillRectangle(New SolidBrush(Color.Gray), boundingBox)
            Case ButtonStatus.ButtonHighlighted
                context.FillRectangle(New SolidBrush(Color.LightPink), boundingBox)
        End Select
        context.DrawRectangle(Pens.Black, Rectangle.Round(boundingBox))
    End Sub

End Class

'Public Class CGButtonSprite : Inherits CGButton
'    Private normalSprite_ As CGSprite
'    Private selectedSprite_ As CGSprite
'    Private highlightedSprite_ As CGSprite
'End Class

'Public Class CGButtonLabel : Inherits CGButton

'End Class

'Public Class CGButtonToggle : Inherits CGButton

'End Class
