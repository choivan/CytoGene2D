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

	Private notifications As Hashtable

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

		Public Function getObservers() As ArrayList
			Return observers
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

		Public Function containObserver(ByVal observer As IObserver) As Boolean
			Return observers.Contains(observer)
		End Sub

		Public Function isEmpty() As Boolean 'no one is listening'
			Return observers.Count == 0
		End Sub
	End Class

	' VBisshitdesignedlanguage '
'	Private Class NotificationEqualityComparer Implements IEqualityComparer (Of Notification)
'		Public Overloads Function Equals(ByVal n1 As Notification, ByVal n2 As Notification) _
'	                   As Boolean Implements IEqualityComparer(Of Notification).Equals
'	        ' same name but different objects'           
'	        If n1.getName() = n2.getName() And n1 != n2 Then 
'	            Return True 
'	        Else 
'	            Return False 
'	        End If 
'    	End Function 
'
'	    Public Overloads Function GetHashCode(ByVal n As Notification) _
'	                As Integer Implements IEqualityComparer(Of Notification).GetHashCode
'	        Return n.getName().GetHashCode()
'	    End Function 
'	End Class

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
		notifications = New Hashtable
	End Sub

	Public Sub registerObserverForNotification(ByVal observer As IObserver, ByVal notificationName As String)
		If notifications.ContainsKey(notificationName) Then
			Dim n As Notification = notifications.Item(notificationName)
			n.addObserver(observer)
		Else
			Dim n As New Notification(notificationName)
			n.addObserver(observer)
			notifications.Add(notificationName, n)
		End If
	End Sub

	Public Sub unregisterObserverForNotification(ByVal observer As IObserver, ByVal notificationName As String)
		If notifications.ContainsKey(notificationName) Then
			Dim n As Notification = notifications.Item(notificationName)
			n.removeObserver(observer)
		End If
	End Sub

	Public Sub unregisterObserverForAllNotification(ByVal observer As IObserver)
		Dim vNotifs As ICollection = notifications.Values
		Dim vNotifArray(notifications.Count - 1) As Notification
		vNotifs.CopyTo(vNotifArray, 0)
		For Each n As Notification in vNotifArray
			If n.containObserver(observer) Then
				n.removeObserver(observer)
				If n.isEmpty() Then ' remove the notification if no one is listening to it'
					notifications.Remove(n.getName())
				End If
			End If
		Next
	End Sub

	Public Sub postNotification(ByVal sender As Object, ByVal notificationName As String, ByVal info As Object)
		Dim n As Notification = notifications.Item(notificationName)
		Dim observers As ArrayList = n.getObservers()
		'todo'
	End Sub
End Class