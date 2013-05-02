Public Class CGDNADoubleStrand : Inherits CGNode
    Private strand1_ As CGDNASingleStrand
    Private strand2_ As CGDNASingleStrand
    Public ReadOnly Property strand1 As CGDNASingleStrand
        Get
            Return strand1_
        End Get
    End Property
    Public ReadOnly Property strand2 As CGDNASingleStrand
        Get
            Return strand2_
        End Get
    End Property

    Sub New(ByVal startNodeCenter1 As PointF, ByVal startNodeCenter2 As PointF,
            ByVal nodeColor1 As Color, ByVal nodeColor2 As Color,
            ByVal nodeRadius As Single, ByVal size As Integer, ByVal gap As Single)
        strand1_ = New CGDNASingleStrand(startNodeCenter1, nodeRadius, nodeColor1, size, gap)
        strand2_ = New CGDNASingleStrand(startNodeCenter2, nodeRadius, nodeColor2, size, gap)
        linkTwoStrands(strand1_, strand2_)
    End Sub

    Public Overrides Sub draw()
        MyBase.draw()
        Dim context As Graphics = CGDirector.sharedDirector.graphicsContext
        Dim node As CGDNANode = strand1_.first
        For i As Integer = 0 To strand1_.count - 1
            If node.linkNode IsNot Nothing AndAlso
                node.nodeColor.ToArgb <> Color.Transparent.ToArgb AndAlso
                node.linkNode.nodeColor.ToArgb <> Color.Transparent.ToArgb Then
                context.DrawLine(Pens.Black, node.center, node.linkNode.center)
            End If
            node = node.nextNode
        Next
        strand1_.draw()
        strand2_.draw()
    End Sub

#Region "Link & unlink operations"
    Private Sub linkTwoStrands(ByVal strand1 As CGDNASingleStrand, ByVal strand2 As CGDNASingleStrand)
        Debug.Assert(strand1_.count = strand2_.count, Me.ToString + ": strands size not match")
        linkNodes(0, strand1_.count)
    End Sub

    Public Sub linkTwoNodes(ByVal node1 As CGDNANode, ByVal node2 As CGDNANode)
        node1.linkNode = node2
        node2.linkNode = node1
    End Sub

    ' link the 'index'th nodes in strand 1 and strand 2
    Public Sub linkTwoNodesAtIndex(ByVal index As Integer)
        Dim node1 As CGDNANode = strand1_.getDNANodeAtIndex(index)
        Dim node2 As CGDNANode = strand2_.getDNANodeAtIndex(index)
        linkTwoNodes(node1, node2)
    End Sub

    ' if start + length > count, then link the nodes till the end
    Public Sub linkNodes(ByVal start As Integer, ByVal length As Integer)
        Dim node1 As CGDNANode = strand1_.getDNANodeAtIndex(start)
        Dim node2 As CGDNANode = strand2_.getDNANodeAtIndex(start)
        While node1.nextNode IsNot Nothing AndAlso node2.nextNode IsNot Nothing AndAlso length > 0
            linkTwoNodes(node1, node2)
            node1 = node1.nextNode
            node2 = node2.nextNode
            length -= 1
        End While
    End Sub

    Public Sub unlinkTwoNodes(ByVal node1 As CGDNANode, ByVal node2 As CGDNANode)
        node1.linkNode = Nothing
        node2.linkNode = Nothing
    End Sub

    Public Sub unlinkTwoNodesAtIndex(ByVal index As Integer)
        Dim node1 As CGDNANode = strand1_.getDNANodeAtIndex(index)
        Dim node2 As CGDNANode = strand2_.getDNANodeAtIndex(index)
        unlinkTwoNodes(node1, node2)
    End Sub

    ' if start + length > count, then unlink the nodes till the end
    Public Sub unlinkNodes(ByVal start As Integer, ByVal length As Integer)
        Dim node1 As CGDNANode = strand1_.getDNANodeAtIndex(start)
        Dim node2 As CGDNANode = strand2_.getDNANodeAtIndex(start)
        While node1.nextNode IsNot Nothing AndAlso node2.nextNode IsNot Nothing AndAlso length > 0
            unlinkTwoNodes(node1, node2)
            node1 = node1.nextNode
            node2 = node2.nextNode
            length -= 1
        End While
    End Sub
#End Region
End Class

'TODO
'Public Class CGDNADoubleStrandCircular : Inherits CGDNADoubleStrand

'End Class

'Public Class CGDNADoubleStrandSpiral : Inherits CGDNADoubleStrand

'End Class