' CGScene is only a abstruct concept.
' But you should always add new node onto a CGScene object.
' A CGScene object should be the root node of all the node in the same scene
Public Class CGScene : Inherits CGNode
    Sub New()
        contentSize = CGDirector.sharedDirector.canvasSize
    End Sub
End Class
