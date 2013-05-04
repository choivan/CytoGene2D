Imports System.Drawing.Imaging
Public Class CGSprite : Inherits CGNode
    Private texture_ As Bitmap
    Public Property texture As Bitmap
        Get
            Return texture_
        End Get
        Set(ByVal value As Bitmap)
            texture_ = value
            contentSize = value.Size ' the size may be changed.
        End Set
    End Property

    Private imageAttributes_ As ImageAttributes ' use the attributes to draw half transparent images.
    Public Overrides Property opacity As Byte
        Get
            Return MyBase.opacity
        End Get
        Set(ByVal value As Byte) ' overrides the set method to change opacity
            MyBase.opacity = value
            Dim matrixItems As Single()() = {
                   New Single() {1, 0, 0, 0, 0},
                   New Single() {0, 1, 0, 0, 0},
                   New Single() {0, 0, 1, 0, 0},
                   New Single() {0, 0, 0, value / Byte.MaxValue, 0},
                   New Single() {0, 0, 0, 0, 1}}
            Dim colorMatrix As New ColorMatrix(matrixItems)
            imageAttributes_ = New ImageAttributes
            imageAttributes_.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap)
            colorMatrix = Nothing 'release it
        End Set
    End Property
    Public ReadOnly Property textureSize As Size
        Get
            Return texture_.Size
        End Get
    End Property

    Sub New(ByVal texture As Bitmap)
        texture_ = texture
        location = PointF.Empty
        contentSize = texture.Size
        opacity = Byte.MaxValue
        userInteractionEnabled = False
    End Sub

    Sub New(ByVal texture As Bitmap, ByVal rect As RectangleF)
        texture_ = texture
        location = rect.Location
        contentSize = rect.Size
        opacity = Byte.MaxValue
        userInteractionEnabled = False
    End Sub

    Sub New(ByVal texture As Bitmap, ByVal centerPoint As PointF)
        texture_ = texture
        contentSize = texture_.Size
        center = centerPoint
        opacity = Byte.MaxValue
        userInteractionEnabled = False
    End Sub

    Public Overrides Sub draw()
        MyBase.draw()
        Dim context As Graphics = CGDirector.sharedDirector.graphicsContext
        If Me.rotation = 0 Then
            context.DrawImage(texture_, Rectangle.Round(boundingBox),
                              0, 0, textureSize.Width, textureSize.Height,
                              GraphicsUnit.Pixel, imageAttributes_)
        Else
            context.TranslateTransform(center.X, center.Y)
            context.RotateTransform(rotation)
            context.TranslateTransform(-center.X, -center.Y)
            context.DrawImage(texture_, Rectangle.Round(boundingBox),
                          0, 0, textureSize.Width, textureSize.Height,
                          GraphicsUnit.Pixel, imageAttributes_)
            context.ResetTransform()
        End If
        
        context = Nothing
    End Sub
End Class
