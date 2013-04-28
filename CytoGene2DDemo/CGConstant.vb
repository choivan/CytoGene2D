Public Module CGConstant
    Public Const kCGPrioritySystem As Integer = Integer.MinValue

    Public Const kCGTopMostZOrder As Integer = Integer.MaxValue
    Public Const kCGBottomMostZOrder As Integer = Integer.MinValue

    Public Const kCGStandardDNANodesGap As Single = 8
    Public Const kCGStandardDNANodesRadius As Single = 5

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
        MouseClick
    End Enum
End Module
