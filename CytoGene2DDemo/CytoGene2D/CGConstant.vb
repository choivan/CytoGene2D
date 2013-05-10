Public Module CGConstant
    Public Const kCGPrioritySystem As Integer = Integer.MinValue

    Public Const kCGTopMostZOrder As Integer = Integer.MaxValue
    Public Const kCGBottomMostZOrder As Integer = Integer.MinValue

    Public Const kCGStandardDNANodesGap As Single = 2
    Public Const kCGStandardDNANodesRadius As Single = 4

    Public Const kCGDefaultAnimationInterval As Single = 0.02

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

    Enum ButtonStatus
        ButtonNormal = 0
        ButtonSelected
        ButtonHighlighted
        ButtonDisabled
    End Enum
End Module
