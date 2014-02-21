Public Interface IObserver
	' sender object helps to determine if it is the right one you want to listen. It is useful when receiver listen to multiple broadcasters
	' notification object helps to determine the right notification to handle. It is useful when receiver register multiple notifications
	' info is the message that broadcaster want to broadcast. Leave it to Nothing if have no message to pass
	Public Sub didObserveNotification(Object sender, String notificationName, Object info)
End Interface

' NotificationCenter is designed for communication purpose among different objects
' Notifications are identified by notification name
' Observers register to the notification center to indicate which notification they want to listen
' Messages may be broadcasted to receivers.
Public Class CGNotificationCenter
	Private Shared sharedInstance_ As CGNotificationCenter
	Private Shared lock_ As New Object

	Private notifications As HashSet(Of Notification)

	Private Class Notification 
		Private name As String
		Private observers As ArrayList

		Sub New(ByVal name As String) 
			this.name = name
			observers = New ArrayList
		End Sub

		Public Function getName() As String
			Return name
		End Function

		Public Sub addObserver(ByVal observer As IObserver)
			observers.Add(observer)
		End	Sub

		Public Sub removeObserver(ByVal observer As IObserver)
			observers.Remove(observer)
		End Sub

		Public Sub removeAllObservers()
			observers.Clear()
		End Sub
	End Class

	' VBisshitdesignedlanguage '
	Private Class NotificationEqualityComparer Implements IEqualityComparer (Of Notification)
		Public Overloads Function Equals(ByVal n1 As Notification, ByVal n2 As Notification) _
	                   As Boolean Implements IEqualityComparer(Of Notification).Equals
	        ' same name but different objects'           
	        If n1.getName() = n2.getName() And n1 != n2 Then 
	            Return True 
	        Else 
	            Return False 
	        End If 
    	End Function 

	    Public Overloads Function GetHashCode(ByVal n As Notification) _
	                As Integer Implements IEqualityComparer(Of Notification).GetHashCode
	        Return n.getName().GetHashCode()
	    End Function 
	End Class

	Public Shared Function sharedNotificationCenter() As CGNotificationCenter
		If sharedInstance_ Is Nothing Then
			SyncLock lock_
				If sharedInstance_ Is Nothing Then
					sharedInstance_ = New CGNotificationCenter
				End If
			End SyncLock
		End If
	End Function

	Protected Sub New()
		notifications = New HashSet(Of Notification)(New NotificationEqualityComparer())
	End Sub

	Public Sub registerObserverForNotification(ByVal observer As IObserver, ByVal notificationName As String)

	End Sub

	Public Sub unregisterObserverForNotification(ByVal obj As Object, ByVal notificationName As String)

	End Sub

	Public Sub unregisterObserverForAllNotification(ByVal obj As Object)

	End Sub

	Public Sub postNotification(ByVal notificationName As String, ByVal info As Object)

	End Sub
End Class