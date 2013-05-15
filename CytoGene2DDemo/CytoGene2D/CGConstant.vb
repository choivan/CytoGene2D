Public Module CGConstant
    Public Const kCGPrioritySystem As Integer = Integer.MinValue

    Public Const kCGTopMostZOrder As Integer = Integer.MaxValue
    Public Const kCGBottomMostZOrder As Integer = Integer.MinValue

    Public Const kCGStandardDNANodesGap As Single = 2
    Public Const kCGStandardDNANodesRadius As Single = 4

    Public Const kCGLineDistanceFixOffset As Single = -2

    Public Const kCGDefaultAnimationInterval As Single = 0.02 ' update per 20 millisecond
    Public Const kCGLongAnimationInterval As Single = 100 ' update per 100 second

    Public Const kCGDefaultFontSize As Single = 14.0
    Public Const kCGFontSizeLarge As Single = 16.0
    Public Const kCGFontSizeSuperLarge As Single = 20.0
    Public Const kCGFontSizeSmall As Single = 12.0

    Public Const kCGDefaultFontName As String = "Times New Roman"
    Public Const kCGFontNameTahoma As String = "Tahoma"
    Public Const kCGFontNameArial As String = "Arial"
    Public Const kCGFontNameImpact As String = "Impact"

    Public kScreenAvailableWidth As Integer = Screen.PrimaryScreen.WorkingArea.Width
    Public kScreenAvailableHeight As Integer = Screen.PrimaryScreen.WorkingArea.Height
    Public kScreenFullWidth As Integer = Screen.PrimaryScreen.Bounds.Width
    Public kScreenFullHeight As Integer = Screen.PrimaryScreen.Bounds.Height

    Public Structure Margin
        Public left As Single
        Public right As Single
        Public top As Single
        Public bottom As Single
        Sub New(left As Single, right As Single, top As Single, bottom As Single)
            Me.left = left
            Me.right = right
            Me.top = top
            Me.bottom = bottom
        End Sub
    End Structure

    Enum InteractionStatus
        MouseIdle = 0
        MouseDown
        MouseMove
    End Enum

    Enum NodeAppendMode
        HorizontalLeftRight = 0
        HorizontalRightLeft
        VerticalTopBottom
        VerticalBottomTop
    End Enum

    Enum MouseEvent
        MouseDown = 0
        MouseMove
        MouseUp
        'MouseClick ' not in use
    End Enum

    Enum ButtonStatus
        ButtonNormal = 0
        ButtonSelected
        ButtonHighlighted
        ButtonDisabled
    End Enum

    ' basic attributes can stack
    ' advanced attributes are not able to stack
    Enum FontAttribute
        FontAttributeNone = 0
        ' basic attributes
        FontAttributeRegular = 1 << 0 ' \fr
        FontAttributeBold = 1 << 1 ' \fb
        FontAttributeItalic = 1 << 2 ' \fi
        FontAttributeUnderLine = 1 << 3 ' \fu
        FontAttributeColorBlack = 1 << 4 ' \ck
        FontAttributeColorRed = 1 << 5 ' \cr
        FontAttributeColorBlue = 1 << 6 ' \cb

        ' advanced attributes
        FontAttributeOrderedList = 1 << 10
        FontAttributeUnorderedList = 1 << 11

        ' more advanced (fixed) attributes. 
        ' the attributes list below are fixed
        FontAttributeTitle1 = 1 << 15
        FontAttributeTitle2 = 1 << 16
        FontAttributeTitle3 = 1 << 17
        FontAttributeImage = 1 << 18
    End Enum

    Structure AttributedString
        Public content As String
        Public attribute As FontAttribute
    End Structure

    Public Function containAttribute(ByVal base As FontAttribute, ByVal testCase As FontAttribute) As Boolean
        Return (base And testCase)
    End Function

    Public Function getSubFontAttributes(ByVal base As FontAttribute) As List(Of FontAttribute)
        Dim subAtts As New List(Of FontAttribute)
        Dim enumArray As Array = System.[Enum].GetValues(GetType(FontAttribute))
        Dim result As Integer = 0
        For Each att As FontAttribute In enumArray
            result = base And att
            If result <> 0 Then
                subAtts.Add(att)
            End If
        Next

        Return subAtts
    End Function

End Module
