' this is a singleton class
Imports System.Text.RegularExpressions
Public Class CGTextRender
    Private stringFormat_ As StringFormat
    Private fontSetted_ As Boolean = False
    Private fontName_ As String
    Private fontSize_ As Single
    Private basicRegularFont_ As Font
    Private basicBoldFont_ As Font
    Private basicItalicFont_ As Font
    Private basicUnderLineFont_ As Font
    Private currentFontStyle_ As Font = Nothing
    Private currentFontColor_ As Brush = Nothing
    Private basicFontHeight_ As Single
    Private blackBrush_ As Brush = Brushes.Black
    Private blueBrush_ As Brush = Brushes.Blue
    Private redBrush_ As Brush = Brushes.Red

    Private currentStartLocation_ As PointF

    Private trailingShadowWidth_ As Single

    Private orderedListNumber_ As Integer = 0
    Public Property orderedListNumber As Integer
        Get
            Return orderedListNumber_
        End Get
        Set(value As Integer)
            orderedListNumber_ = value
        End Set
    End Property

    Private Const kTitle1FontSizeOffset As Integer = 6
    Private Const kTitle2FontSizeOffset As Integer = 4
    Private Const kTitle3FontSizeOffset As Integer = 2

    Public Sub resetOrderedListNumber()
        orderedListNumber_ = 0
    End Sub

    Sub New()
        stringFormat_ = New StringFormat
        stringFormat_.FormatFlags = StringFormatFlags.MeasureTrailingSpaces
    End Sub

    Public Sub render(g As Graphics, attributedLine As List(Of AttributedString),
                      layoutArea As RectangleF)
        If attributedLine.Count = 0 Then
            Return
        End If

        currentStartLocation_.X = layoutArea.Left
        currentStartLocation_.Y = layoutArea.Top
        Dim indentWidth As Single = 0

        Dim aString As AttributedString = attributedLine(0)
        If aString.attribute = FontAttribute.FontAttributeTitle1 Then
            resetOrderedListNumber()
            Dim title1Font As New Font(fontName_, fontSize_ + kTitle1FontSizeOffset, FontStyle.Bold)
            g.DrawString(aString.content, title1Font, blackBrush_, layoutArea)
        ElseIf aString.attribute = FontAttribute.FontAttributeTitle2 Then
            resetOrderedListNumber()
            Dim title1Font As New Font(fontName_, fontSize_ + kTitle2FontSizeOffset, FontStyle.Bold)
            g.DrawString(aString.content, title1Font, blackBrush_, layoutArea)
        ElseIf aString.attribute = FontAttribute.FontAttributeTitle3 Then
            resetOrderedListNumber()
            Dim title1Font As New Font(fontName_, fontSize_ + kTitle3FontSizeOffset, FontStyle.Bold)
            g.DrawString(aString.content, title1Font, blackBrush_, layoutArea)
        ElseIf aString.attribute = FontAttribute.FontAttributeOrderedList Then
            orderedListNumber_ += 1
            indentWidth = getWidthOfAttributedString(g, aString)
            Dim orderNumberIndent As String
            If orderedListNumber_ > 9 Then ' assume order list has less than 100 items
                orderNumberIndent = "    "
            Else
                orderNumberIndent = "     "
            End If
            g.DrawString(orderNumberIndent + orderedListNumber_.ToString + ".", _
                         basicRegularFont_, blackBrush_, layoutArea, stringFormat_)
            currentStartLocation_.X += indentWidth
            layoutArea = New RectangleF(currentStartLocation_.X, currentStartLocation_.Y, _
                                        layoutArea.Width - indentWidth, layoutArea.Height)
        ElseIf aString.attribute = FontAttribute.FontAttributeUnorderedList Then
            resetOrderedListNumber()
            indentWidth = getWidthOfAttributedString(g, aString)
            g.DrawString(aString.content, basicRegularFont_, blackBrush_, layoutArea)
            currentStartLocation_.X += indentWidth
            layoutArea = New RectangleF(currentStartLocation_.X, currentStartLocation_.Y, _
                                        layoutArea.Width - indentWidth, layoutArea.Height)
        ElseIf containAttribute(aString.attribute, FontAttribute.FontAttributeImage) Then
            g.DrawImage(New Bitmap(aString.content), layoutArea)
        Else ' render content
            resetOrderedListNumber()
            synthesisFontStyleAndColor(aString.attribute)
            renderContent(g, aString, currentStartLocation_, indentWidth, layoutArea)
        End If

        If attributedLine.Count > 1 Then
            For i As Integer = 1 To attributedLine.Count - 1
                aString = attributedLine(i)
                synthesisFontStyleAndColor(aString.attribute)
                renderContent(g, aString, currentStartLocation_, indentWidth, layoutArea)
            Next
        End If
    End Sub

    ' using current font and color to render
    ' TODO: refactor this method if have time. ugly.. really...
    Private Sub renderContent(g As Graphics, aString As AttributedString,
                              startLocation As PointF, indentWidth As Single,
                              layoutRect As RectangleF)
        Dim stringWidth As Single = getWidthOfAttributedString(g, aString)
        If stringWidth + startLocation.X - indentWidth > (layoutRect.Width + layoutRect.Left) Then ' goes to the next lines
            Dim remainWidth As Single = layoutRect.Width + layoutRect.Left - startLocation.X - indentWidth
            Dim ratio As Single = remainWidth / stringWidth
            Dim strLength As Integer = aString.content.Length
            Dim spiltIndex As Integer = Math.Floor(strLength * ratio)
            spiltIndex = IIf(spiltIndex < 0, 0, spiltIndex)
            While spiltIndex > 0 AndAlso aString.content(spiltIndex) <> " "c
                spiltIndex -= 1
            End While
            Dim subStr1 As String = aString.content.Substring(0, spiltIndex)
            Dim subStr1Width As Single = getWidthOfAttributedString(g, subStr1, aString.attribute)
            While subStr1Width > remainWidth                    ' the reason of this while loop to compare the new substring length with total remain length is
                ratio = remainWidth / subStr1Width              ' the font used are not fixed-width mostly. 
                spiltIndex = Math.Floor(subStr1.Length * ratio) ' But calculating the ratio and finding the spilt index may result the substring have greater physical width.
                spiltIndex = IIf(spiltIndex < 0, 0, spiltIndex)
                While spiltIndex > 0 AndAlso aString.content(spiltIndex) <> " "c
                    spiltIndex -= 1
                End While
                subStr1 = aString.content.Substring(0, spiltIndex)
                subStr1Width = getWidthOfAttributedString(g, subStr1, aString.attribute)
            End While
            Dim subStr2 As String = aString.content.Substring(spiltIndex + 1)
            g.DrawString(subStr1, currentFontStyle_, currentFontColor_, startLocation)
            currentStartLocation_.X = layoutRect.Left + indentWidth
            currentStartLocation_.Y += basicFontHeight_
            stringWidth = getWidthOfAttributedString(g, subStr2, aString.attribute)
            While stringWidth > (layoutRect.Width - indentWidth)
                remainWidth = layoutRect.Width - indentWidth
                ratio = remainWidth / stringWidth
                strLength = subStr2.Length
                spiltIndex = Math.Floor(strLength * ratio)
                spiltIndex = IIf(spiltIndex < 0, 0, spiltIndex)
                While spiltIndex > 0 AndAlso subStr2(spiltIndex) <> " "c
                    spiltIndex -= 1
                End While
                subStr1 = subStr2.Substring(0, spiltIndex)
                subStr1Width = getWidthOfAttributedString(g, subStr1, aString.attribute)
                While subStr1Width > remainWidth
                    ratio = remainWidth / subStr1Width
                    spiltIndex = Math.Floor(subStr1.Length * ratio)
                    spiltIndex = IIf(spiltIndex < 0, 0, spiltIndex)
                    While spiltIndex > 0 AndAlso subStr2(spiltIndex) <> " "c
                        spiltIndex -= 1
                    End While
                    subStr1 = subStr2.Substring(0, spiltIndex)
                    subStr1Width = getWidthOfAttributedString(g, subStr1, aString.attribute)
                End While
                subStr2 = subStr2.Substring(spiltIndex + 1)
                g.DrawString(subStr1, currentFontStyle_, currentFontColor_, currentStartLocation_)
                stringWidth = getWidthOfAttributedString(g, subStr2, aString.attribute)
                currentStartLocation_.X = layoutRect.Left + indentWidth
                currentStartLocation_.Y += basicFontHeight_
            End While
            If subStr2.Length > 0 Then
                g.DrawString(subStr2, currentFontStyle_, currentFontColor_, currentStartLocation_)
                currentStartLocation_.X += getWidthOfAttributedString(g, subStr2, aString.attribute)
            End If
        Else ' within current line
            g.DrawString(aString.content, currentFontStyle_, currentFontColor_, startLocation)
            Dim lineWidth As Single = getWidthOfAttributedString(g, aString)
            currentStartLocation_.X += lineWidth
        End If
    End Sub

    Private Function needHyphen(s As String, index As Integer) As Boolean
        If index > s.Length Or index = 0 Then
            Return False
        End If
        Dim left As String = s.Substring(index - 1, 1)
        Dim right As String = s.Substring(index, 1)
        If Regex.IsMatch(left, "[a-zA-Z0-9]") And Regex.IsMatch(right, "[a-zA-Z0-9]") Then
            Return True
        End If
        Return False
    End Function

    ' after synthesis, then you can use currentfontstyle and currentfontcolor
    Private Sub synthesisFontStyleAndColor(att As FontAttribute)
        currentFontStyle_ = Nothing
        currentFontColor_ = Nothing
        If containAttribute(att, FontAttribute.FontAttributeRegular) Then
            currentFontStyle_ = basicRegularFont_
        End If
        If containAttribute(att, FontAttribute.FontAttributeItalic) Then
            currentFontStyle_ = basicItalicFont_
            If containAttribute(att, FontAttribute.FontAttributeUnderLine) Then
                Dim underlineItalicFont As New Font(fontName_, fontSize_, FontStyle.Underline Or FontStyle.Italic)
                currentFontStyle_ = underlineItalicFont
            End If
        ElseIf containAttribute(att, FontAttribute.FontAttributeUnderLine) Then
            currentFontStyle_ = basicUnderLineFont_
        End If
        If containAttribute(att, FontAttribute.FontAttributeBold) Then
            currentFontStyle_ = basicBoldFont_
            If containAttribute(att, FontAttribute.FontAttributeItalic) Then
                Dim boldItalicFont As New Font(fontName_, fontSize_, FontStyle.Bold Or FontStyle.Italic)
                currentFontStyle_ = boldItalicFont
                If containAttribute(att, FontAttribute.FontAttributeUnderLine) Then
                    Dim boldUnderlineItalicFont As New Font(fontName_, fontSize_, FontStyle.Underline Or FontStyle.Underline Or FontStyle.Italic)
                    currentFontStyle_ = boldUnderlineItalicFont
                End If
            ElseIf containAttribute(att, FontAttribute.FontAttributeUnderLine) Then
                Dim boldUnderlineFont As New Font(fontName_, fontSize_, FontStyle.Bold Or FontStyle.Underline)
                currentFontStyle_ = boldUnderlineFont
            End If
        End If
        If containAttribute(att, FontAttribute.FontAttributeColorBlack) Then
            currentFontColor_ = blackBrush_
        ElseIf containAttribute(att, FontAttribute.FontAttributeColorBlue) Then
            currentFontColor_ = blueBrush_
        ElseIf containAttribute(att, FontAttribute.FontAttributeColorRed) Then
            currentFontColor_ = redBrush_
        End If
        If currentFontStyle_ Is Nothing Then
            currentFontStyle_ = basicRegularFont_
        End If
        If currentFontColor_ Is Nothing Then
            currentFontColor_ = blackBrush_
        End If
    End Sub

    Public Sub setFontHeight(g As Graphics)
        If Not fontSetted_ Then Return
        basicFontHeight_ = g.MeasureString("A", basicBoldFont_).Height + kCGLineDistanceFixOffset
        trailingShadowWidth_ = g.MeasureString("..", basicRegularFont_, 50, stringFormat_).Width - g.MeasureString(".", basicRegularFont_, 50, stringFormat_).Width
    End Sub

    Public Sub setBasicFont(fontName As String, fontSize As Single)
        Me.fontName_ = fontName
        Me.fontSize_ = fontSize
        basicRegularFont_ = New Font(fontName, fontSize, FontStyle.Regular)
        basicBoldFont_ = New Font(fontName, fontSize, FontStyle.Bold)
        basicItalicFont_ = New Font(fontName, fontSize, FontStyle.Italic)
        basicUnderLineFont_ = New Font(fontName, fontSize, FontStyle.Underline)
        Me.fontSetted_ = True
    End Sub

    Public Function getSizeOfParagraphWithConstraintSize(g As Graphics,
                                                         line As List(Of AttributedString),
                                                         constraintSize As SizeF) As SizeF
        Dim orderSymbolWidth As Single = 0
        Dim aString As AttributedString = line(0)
        Dim allContent As String = ""
        If aString.attribute = FontAttribute.FontAttributeUnorderedList Then
            orderSymbolWidth = meansureStringWidth(g, aString.content, basicRegularFont_) * 2
            constraintSize = New SizeF(constraintSize.Width - orderSymbolWidth, _
                                       constraintSize.Height)
        ElseIf aString.attribute = FontAttribute.FontAttributeOrderedList Then
            orderSymbolWidth = meansureStringWidth(g, aString.content, basicRegularFont_) * 7
            constraintSize = New SizeF(constraintSize.Width - orderSymbolWidth, _
                                       constraintSize.Height)
        ElseIf aString.attribute = FontAttribute.FontAttributeTitle1 Then
            Dim title1Font As New Font(fontName_, fontSize_ + kTitle1FontSizeOffset, FontStyle.Bold)
            Return g.MeasureString(aString.content, title1Font, constraintSize, stringFormat_)
        ElseIf aString.attribute = FontAttribute.FontAttributeTitle2 Then
            Dim title1Font As New Font(fontName_, fontSize_ + kTitle2FontSizeOffset, FontStyle.Bold)
            Return g.MeasureString(aString.content, title1Font, constraintSize, stringFormat_)
        ElseIf aString.attribute = FontAttribute.FontAttributeTitle3 Then
            Dim title1Font As New Font(fontName_, fontSize_ + kTitle3FontSizeOffset, FontStyle.Bold)
            Return g.MeasureString(aString.content, title1Font, constraintSize, stringFormat_)
        ElseIf containAttribute(aString.attribute, FontAttribute.FontAttributeImage) Then
            Return New Bitmap(aString.content).Size
        Else
            allContent += aString.content
        End If

        ' iterate each attributed string in line. calculate the size.
        Dim hasBold As Boolean = False
        For i As Integer = 1 To line.Count - 1
            aString = line(i)
            allContent += aString.content
            If containAttribute(aString.attribute, FontAttribute.FontAttributeBold) AndAlso Not hasBold Then
                hasBold = True
            End If
        Next
        Dim newSize As SizeF = g.MeasureString(allContent, IIf(hasBold, basicBoldFont_, basicRegularFont_), constraintSize, stringFormat_)
        Return New SizeF(constraintSize.Width + orderSymbolWidth,
                         newSize.Height)
    End Function

    Private Function getWidthOfAttributedString(g As Graphics, s As String, attribute As FontAttribute) As Single
        Dim aString As AttributedString
        aString.content = s
        aString.attribute = attribute
        Return getWidthOfAttributedString(g, aString)
    End Function

    Public Function getWidthOfAttributedString(g As Graphics, aString As AttributedString) As Single
        If aString.attribute = FontAttribute.FontAttributeTitle1 Then
            Dim title1Font As New Font(fontName_, fontSize_ + kTitle1FontSizeOffset, FontStyle.Bold)
            Return meansureStringWidth(g, aString.content, title1Font)
        ElseIf aString.attribute = FontAttribute.FontAttributeTitle2 Then
            Dim title2Font As New Font(fontName_, fontSize_ + kTitle2FontSizeOffset, FontStyle.Bold)
            Return meansureStringWidth(g, aString.content, title2Font)
        ElseIf aString.attribute = FontAttribute.FontAttributeTitle3 Then
            Dim title3Font As New Font(fontName_, fontSize_ + kTitle3FontSizeOffset, FontStyle.Bold)
            Return measureString(g, aString.content, title3Font).Width
        ElseIf aString.attribute = FontAttribute.FontAttributeOrderedList Then
            Return meansureStringWidth(g, aString.content, basicRegularFont_) * 7
        ElseIf aString.attribute = FontAttribute.FontAttributeUnorderedList Then
            Return meansureStringWidth(g, aString.content, basicRegularFont_) * 2
        ElseIf containAttribute(aString.attribute, FontAttribute.FontAttributeImage) Then
            Return New Bitmap(aString.content).Size.Width
        Else
            If containAttribute(aString.attribute, FontAttribute.FontAttributeRegular) Then
                If containAttribute(aString.attribute, FontAttribute.FontAttributeItalic) Then
                    Return meansureStringWidth(g, aString.content, basicItalicFont_)
                End If
                Return meansureStringWidth(g, aString.content, basicRegularFont_)
            End If
            If containAttribute(aString.attribute, FontAttribute.FontAttributeBold) Then
                If containAttribute(aString.attribute, FontAttribute.FontAttributeItalic) Then
                    Dim boldItalicFont As New Font(fontName_, fontSize_, FontStyle.Bold Or FontStyle.Italic)
                    Return meansureStringWidth(g, aString.content, boldItalicFont)
                End If
                Return meansureStringWidth(g, aString.content, basicBoldFont_)
            End If
            If containAttribute(aString.attribute, FontAttribute.FontAttributeItalic) Then
                Return meansureStringWidth(g, aString.content, basicItalicFont_)
            End If
            ' default case. may never reach here.
            Return meansureStringWidth(g, aString.content, basicRegularFont_)
        End If
    End Function

    Private Function measureString(g As Graphics, text As String, f As Font) As SizeF
        Return g.MeasureString(text, f, PointF.Empty, stringFormat_)
    End Function

    Private Function meansureStringWidth(g As Graphics, text As String, f As Font) As Single
        Return g.MeasureString(text, f, PointF.Empty, stringFormat_).Width - trailingShadowWidth_
    End Function
End Class
