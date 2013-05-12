﻿' this is a singleton class
Imports System.Text.RegularExpressions
Public Class CGTextRender
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
                         basicRegularFont_, blackBrush_, layoutArea)
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
    Private Sub renderContent(g As Graphics, aString As AttributedString,
                              startLocation As PointF, indentWidth As Single,
                              layoutRect As RectangleF)
        Dim stringWidth As Single = getWidthOfAttributedString(g, aString)
        If stringWidth + startLocation.X - indentWidth > layoutRect.Width Then ' goes to the next lines
            Dim ratio As Single = (layoutRect.Width - startLocation.X - indentWidth) / stringWidth
            Dim strLength As Integer = aString.content.Length
            Dim spiltIndex As Integer = Math.Floor(strLength * ratio)
            spiltIndex = IIf(spiltIndex < 0, 0, spiltIndex)
            While spiltIndex > 0 AndAlso aString.content(spiltIndex) <> " "c
                spiltIndex -= 1
            End While
            Dim subStr1 As String = aString.content.Substring(0, spiltIndex)
            Dim subStr2 As String = aString.content.Substring(spiltIndex + 1)
            g.DrawString(subStr1, currentFontStyle_, currentFontColor_, startLocation)
            currentStartLocation_.X = layoutRect.Left
            currentStartLocation_.Y += basicFontHeight_
            stringWidth = getWidthOfAttributedString(g, subStr2, aString.attribute)
            While stringWidth > layoutRect.Width
                ratio = layoutRect.Width / stringWidth
                strLength = subStr2.Length
                spiltIndex = Math.Floor(strLength * ratio)
                spiltIndex = IIf(spiltIndex < 0, 0, spiltIndex)
                While spiltIndex > 0 AndAlso subStr2(spiltIndex) <> " "c
                    spiltIndex -= 1
                End While
                subStr1 = subStr2.Substring(0, spiltIndex)
                subStr2 = subStr2.Substring(spiltIndex + 1)
                g.DrawString(subStr1, currentFontStyle_, currentFontColor_, currentStartLocation_)
                stringWidth = getWidthOfAttributedString(g, subStr2, aString.attribute)
                currentStartLocation_.X = layoutRect.Left
                currentStartLocation_.Y += basicFontHeight_
            End While
            If subStr2.Length > 0 Then
                g.DrawString(subStr2, currentFontStyle_, currentFontColor_, currentStartLocation_)
                currentStartLocation_.X = layoutRect.Left + getWidthOfAttributedString(g, subStr2, aString.attribute)
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
        basicFontHeight_ = g.MeasureString("A", basicBoldFont_).Height
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

    Public Function getSizeOfLineWithConstraintSize(g As Graphics,
                                                    line As List(Of AttributedString),
                                                    constraintSize As SizeF) As SizeF
        Dim orderSymbolWidth As Single = 0
        Dim aString As AttributedString = line(0)
        Dim allContent As String = ""
        If aString.attribute = FontAttribute.FontAttributeUnorderedList Then
            orderSymbolWidth = g.MeasureString(aString.content, basicRegularFont_, constraintSize).Width * 2
            constraintSize = New SizeF(constraintSize.Width - orderSymbolWidth, _
                                       constraintSize.Height)
        ElseIf aString.attribute = FontAttribute.FontAttributeOrderedList Then
            orderSymbolWidth = g.MeasureString(aString.content, basicRegularFont_, constraintSize).Width * 4
            constraintSize = New SizeF(constraintSize.Width - orderSymbolWidth, _
                                       constraintSize.Height)
        ElseIf aString.attribute = FontAttribute.FontAttributeTitle1 Then
            Dim title1Font As New Font(fontName_, fontSize_ + kTitle1FontSizeOffset, FontStyle.Bold)
            Return g.MeasureString(aString.content, title1Font, constraintSize)
        ElseIf aString.attribute = FontAttribute.FontAttributeTitle2 Then
            Dim title1Font As New Font(fontName_, fontSize_ + kTitle2FontSizeOffset, FontStyle.Bold)
            Return g.MeasureString(aString.content, title1Font, constraintSize)
        ElseIf aString.attribute = FontAttribute.FontAttributeTitle3 Then
            Dim title1Font As New Font(fontName_, fontSize_ + kTitle3FontSizeOffset, FontStyle.Bold)
            Return g.MeasureString(aString.content, title1Font, constraintSize)
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
        Dim newSize As SizeF = g.MeasureString(allContent, IIf(hasBold, basicBoldFont_, basicRegularFont_), constraintSize)
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
            Return g.MeasureString(aString.content, title1Font).Width
        ElseIf aString.attribute = FontAttribute.FontAttributeTitle2 Then
            Dim title1Font As New Font(fontName_, fontSize_ + kTitle2FontSizeOffset, FontStyle.Bold)
            Return g.MeasureString(aString.content, title1Font).Width
        ElseIf aString.attribute = FontAttribute.FontAttributeTitle3 Then
            Dim title1Font As New Font(fontName_, fontSize_ + kTitle3FontSizeOffset, FontStyle.Bold)
            Return g.MeasureString(aString.content, title1Font).Width
        ElseIf aString.attribute = FontAttribute.FontAttributeOrderedList Then
            Return g.MeasureString(aString.content, basicRegularFont_).Width * 4
        ElseIf aString.attribute = FontAttribute.FontAttributeUnorderedList Then
            Return g.MeasureString(aString.content, basicRegularFont_).Width * 2
        Else
            If containAttribute(aString.attribute, FontAttribute.FontAttributeRegular) Then
                If containAttribute(aString.attribute, FontAttribute.FontAttributeItalic) Then
                    Return g.MeasureString(aString.content, basicItalicFont_).Width
                End If
                Return g.MeasureString(aString.content, basicRegularFont_).Width
            End If
            If containAttribute(aString.attribute, FontAttribute.FontAttributeBold) Then
                If containAttribute(aString.attribute, FontAttribute.FontAttributeItalic) Then
                    Dim boldItalicFont As New Font(fontName_, fontSize_, FontStyle.Bold Or FontStyle.Italic)
                    Return g.MeasureString(aString.content, boldItalicFont).Width
                End If
                Return g.MeasureString(aString.content, basicBoldFont_).Width
            End If
            If containAttribute(aString.attribute, FontAttribute.FontAttributeItalic) Then
                Return g.MeasureString(aString.content, basicItalicFont_).Width
            End If
            ' default case. may never reach here.
            Return g.MeasureString(aString.content, basicRegularFont_).Width
        End If
    End Function


End Class