Public Class CGLabel : Inherits CGNode
    Public text As String
    Public textFont As Font
    Public textFormat As StringFormat

    Sub New(ByVal text As String, ByVal frame As RectangleF)
        Me.text = text
        boundingBox = frame
        textFont = SystemFonts.DefaultFont
        textFormat = New StringFormat
        textFormat.Alignment = StringAlignment.Center
        textFormat.LineAlignment = StringAlignment.Center
    End Sub

    Public Overrides Sub draw()
        MyBase.draw()
        Dim context As Graphics = CGDirector.sharedDirector.graphicsContext
        context.DrawString(text, textFont, New SolidBrush(Color.Black), boundingBox, textFormat)
        context = Nothing
    End Sub
End Class
