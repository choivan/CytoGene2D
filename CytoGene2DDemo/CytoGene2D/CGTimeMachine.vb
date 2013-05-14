Public Class CGTimeMachine
    Private screenShots_ As List(Of Bitmap)
    Private lastRecordIndex_ As Integer
    Private currentIndex_ As Integer
    Public ReadOnly Property isTimeTraveling As Boolean
        Get
            Return currentIndex_ < lastRecordIndex_ - 1
        End Get
    End Property

    Sub New()
        screenShots_ = New List(Of Bitmap)
        lastRecordIndex_ = 0
        currentIndex_ = 0
    End Sub

    Public Sub record()
        If currentIndex_ <> lastRecordIndex_ Then
            Console.WriteLine(Me.ToString + ": Cannot record while doing time travel.")
            Return
        End If
        screenShots_.Add(CGDirector.sharedDirector.canvasBuff.Clone)
        lastRecordIndex_ += 1
        currentIndex_ = lastRecordIndex_
    End Sub

    Public Function hasPreviousRecord() As Boolean
        Return currentIndex_ > 0
    End Function

    Public Function hasNextRecord() As Boolean
        Dim hasNext As Boolean = isTimeTraveling
        If Not hasNext Then currentIndex_ = lastRecordIndex_
        Return hasNext
    End Function

    Public Function previousRecord() As Bitmap
        If Not hasPreviousRecord() Then Return Nothing
        currentIndex_ -= 1
        Dim image As Bitmap = screenShots_(currentIndex_)
        Return image
    End Function

    Public Function nextRecord() As Bitmap
        If Not hasNextRecord() Then Return Nothing
        currentIndex_ += 1
        Dim image As Bitmap = screenShots_(currentIndex_)
        Return image
    End Function
End Class
