Module CGHelper
    Public Class LabelPool
        Private descriptions_ As List(Of CGLabel)
        Private lastAvailabelIndex_ As Integer
        Private capacity_ As Integer

        Sub New(capacity As Integer)
            capacity_ = capacity
            descriptions_ = New List(Of CGLabel)(capacity)
            For i As Integer = 0 To capacity - 1
                descriptions_.Add(New CGLabel)
            Next
            lastAvailabelIndex_ = 0
        End Sub

        Public Function getNextLabel() As CGLabel
            Dim label As CGLabel = descriptions_(lastAvailabelIndex_)
            lastAvailabelIndex_ = (lastAvailabelIndex_ + 1) Mod capacity_
            Return label
        End Function

        Public Function getLabelAtIndex(index As Integer) As CGLabel
            Return descriptions_(index)
        End Function

        Public Function getAllDescriptions() As List(Of CGLabel)
            Return descriptions_
        End Function

        Public Function getLabelWithText(text As String) As CGLabel
            For Each l As CGLabel In descriptions_
                If l.text = text Then
                    Return l
                End If
            Next
            Return Nothing
        End Function

        Public Sub expandCapacity(newCapacity As Integer)
            If newCapacity <= capacity_ Then Return
            For i As Integer = capacity_ To newCapacity - 1
                descriptions_.Add(New CGLabel)
            Next
            capacity_ = newCapacity
        End Sub

        Public Sub removeAllLabels()
            For Each l As CGLabel In descriptions_
                l.removeFromParentAndCleanup(True)
            Next
        End Sub

        Public Sub removeLabelWithText(text As String)
            Dim l As CGLabel = Me.getLabelWithText(text)
            If l IsNot Nothing Then
                l.removeFromParentAndCleanup(True)
            End If
        End Sub
    End Class

    Public Function fadeoutToRemove(dura As Integer, node As CGNode)
        Return New CGSequence(New CGFadeOut(dura), New CGActionInstant(Sub() node.removeFromParentAndCleanup(True)))
    End Function

    Public Sub formEllipseDoubleStrand(startNode1 As CGDNANode, startNode2 As CGDNANode, size As Integer,
                                       startDegree As Single, angle As Single, a As Single, b As Single, center As PointF, halfStrandGap As Single)
        Dim aDelta As Single = angle / size
        For i As Integer = 0 To size - 1
            Dim dest As PointF = New PointF(center.X + (a + halfStrandGap) * Math.Sin(startDegree), center.Y + (b + halfStrandGap) * Math.Cos(startDegree))
            startNode1.runAction(New CGMoveTo(60, dest))
            dest = New PointF(center.X + (a - halfStrandGap) * Math.Sin(startDegree), center.Y + (b - halfStrandGap) * Math.Cos(startDegree))
            startNode2.runAction(New CGMoveTo(60, dest))
            startDegree += aDelta
            startNode1 = startNode1.nextNode : startNode2 = startNode2.nextNode
        Next
    End Sub

    Public Sub formEllipseSingleStrand(startNode As CGDNANode, size As Integer,
                                       startDegree As Single, angle As Single, a As Single, b As Single, center As PointF)
        Dim aDelta As Single = angle / size
        For i As Integer = 0 To size - 1
            Dim dest As PointF = New PointF(center.X + a * Math.Sin(startDegree), center.Y + b * Math.Cos(startDegree))
            startNode.runAction(New CGMoveTo(60, dest))
            startDegree += aDelta
            startNode = startNode.nextNode
        Next
    End Sub

    ' will remove the donor double strand after inserting
    'Public Sub insertDoubleStrandAfter(target As CGDNADoubleStrand, index As Integer, donor As CGDNADoubleStrand)
    '    Dim n11 As CGDNANode = target.strand1.getDNANodeAtIndex(index)
    '    Dim n12 As CGDNANode = target.strand1.getDNANodeAtIndex(index + 1)
    '    Dim n21 As CGDNANode = target.strand2.getDNANodeAtIndex(index)
    '    Dim n22 As CGDNANode = target.strand2.getDNANodeAtIndex(index + 1)
    '    n11.nextNode = donor.strand1.first : donor.strand1.first.lastNode = n11
    '    n12.lastNode = donor.strand1.last : donor.strand1.last.nextNode = n12
    '    n21.nextNode = donor.strand2.first : donor.strand2.first.lastNode = n21
    '    n22.lastNode = donor.strand2.last : donor.strand2.last.nextNode = n22
    '    target.strand1.count += donor.strand1.count
    '    target.strand2.count += donor.strand2.count
    '    donor.removeFromParentAndCleanup(True)
    'End Sub

    'Public Sub separateDoubleStrandAt(donor As CGDNADoubleStrand, indexStart As Integer, indexEnd As Integer, receipt As CGDNADoubleStrand)
    '    donor.dyeDNADoubleStrand(Color.Transparent, indexStart - 1, 1)
    '    Dim n11 As CGDNANode = donor.strand1.getDNANodeAtIndex(indexStart)
    '    Dim n21 As CGDNANode = donor.strand2.getDNANodeAtIndex(indexStart)
    '    receipt.strand1.first = n11
    '    receipt.strand2.first = n21
    '    Dim n12 As CGDNANode = donor.strand1.getDNANodeAtIndex(indexEnd)
    '    Dim n22 As CGDNANode = donor.strand2.getDNANodeAtIndex(indexEnd)
    '    receipt.strand1.last = n12
    '    receipt.strand2.last = n22
    '    n11.lastNode.nextNode = n12.nextNode
    '    n12.nextNode.lastNode = n11.lastNode
    '    n21.lastNode.nextNode = n22.nextNode
    '    n22.nextNode.lastNode = n21.lastNode
    '    receipt.strand1.first.lastNode = Nothing : receipt.strand1.last.nextNode = Nothing
    '    receipt.strand2.first.lastNode = Nothing : receipt.strand2.last.nextNode = Nothing
    '    receipt.strand1.count = indexEnd - indexStart + 1
    '    receipt.strand2.count = indexEnd - indexStart + 1
    '    donor.strand1.count -= receipt.strand1.count
    '    donor.strand2.count -= receipt.strand1.count
    'End Sub
End Module
