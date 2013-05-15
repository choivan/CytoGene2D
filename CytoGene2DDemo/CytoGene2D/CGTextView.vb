Public Class CGTextView : Inherits CGNode
    Private textRenderer_ As CGTextRender
    Public ReadOnly Property textRenderer As CGTextRender
        Get
            Return textRenderer_
        End Get
    End Property
    Private textParser_ As CGTextParser
    Public ReadOnly Property textParser As CGTextParser
        Get
            Return textParser_
        End Get
    End Property
    Private pagesStartingIndices_ As List(Of Integer)
    Private pagesEndingIndices_ As List(Of Integer)
    Private currentPage_ As Integer
    Private paragraphVerticalOffset_ As Single
    Private margin_ As Margin
    Private originalFrame_ As RectangleF
    Private orderedListStartNumber_ As Integer

    Sub New(frame As RectangleF)
        setFrame(frame)
        textParser_ = New CGTextParser
        textRenderer_ = New CGTextRender
        setBasicFont(kCGDefaultFontName, kCGDefaultFontSize)
        currentPage_ = 0
        pagesStartingIndices_ = New List(Of Integer) : pagesStartingIndices_.Add(0)
        pagesEndingIndices_ = New List(Of Integer) : pagesEndingIndices_.Add(0)
        paragraphVerticalOffset_ = 0 
    End Sub

    Public Sub parseFile(fileName As String)
        textParser.processFile(fileName)
        paragraphVerticalOffset_ = textRenderer.getSizeOfParagraphWithConstraintSize(CGDirector.sharedDirector.graphicsContext,
                                                                                     textParser.attributedParagraphs(0), contentSize).Height
    End Sub

    Public Sub setFrame(frame As RectangleF)
        originalFrame_ = frame
        setPageMargin(New Margin(10, 10, 10, 10))
    End Sub

    Public Sub setBasicFont(fontName As String, fontSize As Single)
        textRenderer_.setBasicFont(fontName, fontSize)
        textRenderer_.setFontHeight(CGDirector.sharedDirector.graphicsContext)
    End Sub

    Public Sub setPageMargin(margin As Margin)
        margin_ = margin
        Dim newFrame As RectangleF = New RectangleF(originalFrame_.X + margin_.left,
                                                    originalFrame_.Y + margin_.top,
                                                    originalFrame_.Width - margin_.left - margin_.right,
                                                    originalFrame_.Height - margin_.top - margin_.bottom)
        boundingBox = newFrame
    End Sub

    Public Function hasNext() As Boolean
        Dim index As Integer = pagesEndingIndices_(currentPage_) + 1
        Return index < textParser.NumberOfParagraphs
    End Function

    Public Function hasLast() As Boolean
        Dim index As Integer = pagesEndingIndices_(currentPage_) - 1
        Return index >= 0
    End Function

    Public Sub showNextParagraph()
        Dim index As Integer = pagesEndingIndices_(currentPage_) + 1
        Dim size As SizeF = textRenderer.getSizeOfParagraphWithConstraintSize(CGDirector.sharedDirector.graphicsContext,
                                                                         textParser.attributedParagraphs(index), contentSize)
        If paragraphVerticalOffset_ + size.Height > boundingBox.Height Then ' goes to next page
            orderedListStartNumber_ = textRenderer.orderedListNumber ' fix ordered list number continous growing issue
            paragraphVerticalOffset_ = size.Height
            currentPage_ += 1
            pagesStartingIndices_.Add(index)
            pagesEndingIndices_.Add(index)
        Else
            paragraphVerticalOffset_ += size.Height
            pagesEndingIndices_(pagesEndingIndices_.Count - 1) = index
        End If
    End Sub

    Public Overrides Sub draw()
        Dim context As Graphics = CGDirector.sharedDirector.graphicsContext
        Dim startIndex As Integer = pagesStartingIndices_(currentPage_)
        Dim endIndex As Integer = pagesEndingIndices_(currentPage_)
        Dim verticalOffset As Single = boundingBox.Y
        textRenderer.orderedListNumber = orderedListStartNumber_
        For i As Integer = startIndex To endIndex
            Dim lineSize As SizeF = textRenderer.getSizeOfParagraphWithConstraintSize(context,
                                                                                 textParser.attributedParagraphs(i),
                                                                                 boundingBox.Size)
            textRenderer.render(context,
                                textParser.attributedParagraphs(i),
                                New RectangleF(boundingBox.X, verticalOffset, lineSize.Width, lineSize.Height))
            verticalOffset += lineSize.Height
        Next
        context = Nothing
    End Sub
End Class
