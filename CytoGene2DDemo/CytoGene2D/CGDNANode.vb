Public Class CGDNANode : Inherits CGNode
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
    Public nextNode As CGDNANode = Nothing
    Public lastNode As CGDNANode = Nothing
    Public linkNode As CGDNANode = Nothing ' a link node is the node that is located in the pairing strand

    Sub New(ByVal radius As Single, ByVal color As Color, ByVal centerPoint As PointF)
        contentSize = New SizeF(radius * 2, radius * 2)
        opacity = 1.0F
        nodeColor = color
        userInteractionEnabled = False
        center = centerPoint
    End Sub

    Public Overrides Sub draw()
        MyBase.draw()
        Dim context As Graphics = CGDirector.sharedDirector.graphicsContext
        context.FillEllipse(New SolidBrush(color_), boundingBox)
        context = Nothing
    End Sub

End Class
