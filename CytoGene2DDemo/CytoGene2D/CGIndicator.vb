Public Class CGIndicator : Inherits CGNode
    Public Enum ShapeStyle
        Box
        Circle
    End Enum
    Private shape_ As ShapeStyle
    Public WriteOnly Property shape As ShapeStyle
        Set(value As ShapeStyle)
            shape_ = value
        End Set
    End Property
    Private color_ As Color
    Public Property nodeColor As Color
        Get
            Return color_
        End Get
        Set(ByVal value As Color)
            If MyBase.opacity < 255 Then
                Dim r As Byte = value.R
                Dim g As Byte = value.G
                Dim b As Byte = value.B
                Dim a As Byte = value.A
                a = Math.Round(a * opacity)
                color_ = Color.FromArgb(a, r, g, b)
            Else
                color_ = value
            End If

        End Set
    End Property
    Public Overrides Property opacity As Byte
        Get
            Return MyBase.opacity
        End Get
        Set(ByVal value As Byte) ' overrides set method to change color
            MyBase.opacity = value
            Dim r As Byte = color_.R
            Dim g As Byte = color_.G
            Dim b As Byte = color_.B
            color_ = Color.FromArgb(opacity, r, g, b)
        End Set
    End Property

    Sub New(ByVal rect As RectangleF, ByVal color As Color)
        contentSize = rect.Size
        location = rect.Location
        opacity = 1.0F
        nodeColor = color
        shape_ = ShapeStyle.Box
    End Sub

    Public Overrides Sub draw()
        MyBase.draw()
        Dim context As Graphics = CGDirector.sharedDirector.graphicsContext
        Select Case shape_
            Case ShapeStyle.Box
                context.DrawRectangle(New Pen(color_, 2.0F), Rectangle.Round(boundingBox))
            Case ShapeStyle.Circle
                context.DrawEllipse(New Pen(color_, 2.0F), Rectangle.Round(boundingBox))
        End Select
        context = Nothing
    End Sub
End Class
