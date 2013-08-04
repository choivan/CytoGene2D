Imports System.Text.RegularExpressions
Imports System.IO

Public Class CGTextParser
    Public attributedParagraphs As New List(Of List(Of AttributedString))

    Private rawParagraphs_ As New List(Of String)
    Private defaultAttribute_ As FontAttribute = FontAttribute.FontAttributeRegular
    Private currentFontStyle_ As FontAttribute = defaultAttribute_
    Private currentFontColor_ As FontAttribute = FontAttribute.FontAttributeColorBlack

    Public ReadOnly Property NumberOfParagraphs
        Get
            Return attributedParagraphs.Count
        End Get
    End Property

    Public Sub New()

    End Sub

    Public Sub processFile(fileName As String)
        If File.Exists(fileName) Then
            Dim reader As New StreamReader(fileName)
            Do While Not reader.EndOfStream
                Dim s As String = reader.ReadLine
                If s.Length > 0 Then
                    rawParagraphs_.Add(s)
                End If
            Loop
            reader.Close()

            processText()
        Else
            Console.WriteLine("File: <" + fileName + "> not exist.")
        End If
    End Sub

    Private Sub processText()
        If rawParagraphs_.Count = 0 Then
            Return
        End If

        For i As Integer = 0 To rawParagraphs_.Count - 1
            Dim attributedLine As New List(Of AttributedString)

            Dim rawString As String = rawParagraphs_(i)
            Dim chars() As Char = rawString.ToCharArray
            Dim index As Integer = 0

            ' title
            If chars(index) = ">"c Then
                While index < 3 And index < chars.Length - 1 And chars(index) = ">"c
                    index += 1
                End While

                Dim astring As AttributedString
                If index = 0 Then
                    astring.attribute = currentFontColor_ Or currentFontStyle_ 'has color and font style
                ElseIf index = 1 Then
                    astring.attribute = FontAttribute.FontAttributeTitle1
                ElseIf index = 2 Then
                    astring.attribute = FontAttribute.FontAttributeTitle2
                Else
                    astring.attribute = FontAttribute.FontAttributeTitle3
                End If
                astring.content = rawString.Substring(index)
                attributedLine.Add(astring)
                attributedParagraphs.Add(attributedLine)
                Continue For
            End If

            ' unordered list
            If chars(index) = "*"c Then
                Dim astring As AttributedString
                astring.attribute = FontAttribute.FontAttributeUnorderedList
                astring.content = "• "
                index += 1
                attributedLine.Add(astring)
                parseContent(New String(chars, index, chars.Length - index), attributedLine)
                Continue For
            End If

            ' ordered list
            If chars(index) = "#"c Then
                Dim astring As AttributedString
                astring.attribute = FontAttribute.FontAttributeOrderedList
                astring.content = "."
                index += 1
                attributedLine.Add(astring)
                parseContent(New String(chars, index, chars.Length - index), attributedLine)
                Continue For
            End If

            parseContent(rawString, attributedLine)
        Next
    End Sub

    Private Sub parseContent(rawString As String, attributedLine As List(Of AttributedString))
        Dim regex As New Regex("(.*?)(<:[^>]+>|\Z)")
        Dim matchResultColletion As MatchCollection = regex.Matches(rawString)
        For Each matchResult As Match In matchResultColletion
            Dim s As String = matchResult.Value ' every s will at most contain one attribute tag "<:xxx>"
            If s.Length <> 0 Then
                Dim spiltIndex As Integer = s.IndexOf("<:")
                If spiltIndex = -1 Then ' no attribute tag found
                    Dim astring As AttributedString
                    astring.content = s
                    astring.attribute = currentFontColor_ Or currentFontStyle_
                    attributedLine.Add(astring)
                ElseIf spiltIndex = 0 Then
                    Dim subS As String = s.Substring(2)
                    parseAttribute(subS)
                Else
                    Dim subS1 As String = s.Substring(0, spiltIndex)
                    Dim astring As AttributedString
                    astring.attribute = currentFontColor_ Or currentFontStyle_
                    astring.content = subS1
                    attributedLine.Add(astring)

                    Dim subS2 As String = s.Substring(spiltIndex + 2) ' skip <:
                    parseAttribute(subS2)
                End If
            End If
        Next

        attributedParagraphs.Add(attributedLine)
    End Sub

    Private Sub parseAttribute(rawString As String)
        Dim fontStyleBackup As FontAttribute = currentFontStyle_
        Dim fontColorBackup As FontAttribute = currentFontColor_

        currentFontColor_ = FontAttribute.FontAttributeNone
        currentFontStyle_ = FontAttribute.FontAttributeNone

        ' font style
        If rawString.Contains("\fr") Then 'regular
            currentFontStyle_ = currentFontStyle_ Or FontAttribute.FontAttributeRegular ' font style can stack
        End If
        If rawString.Contains("\fb") And currentFontStyle_ <> FontAttribute.FontAttributeRegular Then
            currentFontStyle_ = currentFontStyle_ Or FontAttribute.FontAttributeBold
        End If
        If rawString.Contains("\fi") Then
            currentFontStyle_ = currentFontStyle_ Or FontAttribute.FontAttributeItalic
        End If
        If rawString.Contains("\fu") Then
            currentFontStyle_ = currentFontStyle_ Or FontAttribute.FontAttributeUnderLine
        End If
        If rawString.Contains("\img") Then
            currentFontStyle_ = FontAttribute.FontAttributeImage
        End If
        If rawString.Contains("\ss") Then
            currentFontStyle_ = FontAttribute.FontAttributeSuperScript Or currentFontStyle_
        End If
        ' attribute tag does not contain font style information. so do not change it
        If currentFontStyle_ = FontAttribute.FontAttributeNone Then
            currentFontStyle_ = fontStyleBackup
        End If

        ' font color
        If rawString.Contains("\ck") Then ' color attribute cannot stack.
            currentFontColor_ = FontAttribute.FontAttributeColorBlack
        End If
        If rawString.Contains("\cr") Then ' color attribute cannot stack.
            currentFontColor_ = FontAttribute.FontAttributeColorRed
        End If
        If rawString.Contains("\cb") Then ' color attribute cannot stack.
            currentFontColor_ = FontAttribute.FontAttributeColorBlue
        End If
        ' attribute tag does not contain font color information. so do not change it
        If currentFontColor_ = FontAttribute.FontAttributeNone Then
            currentFontColor_ = fontColorBackup
        End If

    End Sub
End Class
