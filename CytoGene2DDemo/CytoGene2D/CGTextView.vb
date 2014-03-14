Public Class CGTextView : Inherits CGNode
    Private textRenderer_ As CGTextRenderer
    Public ReadOnly Property textRenderer As CGTextRenderer
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

    Public ReadOnly Property lastParagraphIndex As Integer
        Get
            Return pagesEndingIndices_(currentPage_)
        End Get
    End Property
    Public isLocked As Boolean ' if a textView is lock, it will ignore all the show next or show last command

    Sub New(frame As RectangleF)
        setFrame(frame)
        textParser_ = New CGTextParser
        textRenderer_ = New CGTextRenderer
        setBasicFont(kCGDefaultFontName, kCGDefaultFontSize)
        currentPage_ = 0
        pagesStartingIndices_ = New List(Of Integer) : pagesStartingIndices_.Add(0)
        pagesEndingIndices_ = New List(Of Integer) : pagesEndingIndices_.Add(0)
        paragraphVerticalOffset_ = 0
        isLocked = False
    End Sub

    Public Sub parseFile(fileName As String)
        textParser.processFile(fileName)
        paragraphVerticalOffset_ = textRenderer.getSizeOfParagraphWithConstraintSize(CGDirector.sharedDirector.graphicsContext,
                                                                                     textParser.attributedParagraphs(0), contentSize).Height
    End Sub

    Public Sub DEBUG_displayAll()
        pagesStartingIndices_(currentPage_) = 0
        pagesEndingIndices_(currentPage_) = textParser.NumberOfParagraphs - 1
    End Sub

    Public Sub setFrame(frame As RectangleF)
        originalFrame_ = frame
        setPageMargin(New Margin(10, 10, 10, 10))
        If paragraphVerticalOffset_ > boundingBox.Height Then
            refreshLayoutAfterAdjustFrame()
        End If
    End Sub

    Public Sub setFrameAnimated(frame As RectangleF, dura As Integer)
        Dim delta As New PointF(frame.X - originalFrame_.X, frame.Y - originalFrame_.Y)
        Me.runAction(New CGSequence(New CGMoveBy(dura, delta),
                                    New CGActionInstant(
                                        Sub()
                                            setFrame(frame)
                                        End Sub)))
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

    Private Sub refreshLayoutAfterAdjustFrame()
        Dim index As Integer = pagesEndingIndices_(currentPage_)
        Dim size As SizeF = textRenderer.getSizeOfParagraphWithConstraintSize(CGDirector.sharedDirector.graphicsContext,
                                                                         textParser.attributedParagraphs(index), contentSize)
        orderedListStartNumber_ = textRenderer.orderedListNumber
        paragraphVerticalOffset_ = size.Height
        currentPage_ += 1
        pagesStartingIndices_.Add(index)
        pagesEndingIndices_.Add(index)
    End Sub

    Public Sub showNextParagraph()
        If isLocked Then Return
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
        If endIndex = 0 Then 'file is empty
            Return
        End If
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
