' *********** HOW TO USE *************
' 1. * preferred! More clear to view the code.
'Dim action As New CGActionInstant(Sub()
'                                      Console.WriteLine("instant action executed!")
'                                  End Sub)
' 2. * preferred. 
'Private Sub testInstantAction()
'    ' pass lambda expression to instant action
'    Dim action As CGActionInstant = CGActionInstant.actionWithDelegate(Sub()
'                                                                           Console.WriteLine("instant action executed!")
'                                                                       End Sub)
'    'action.takeStep() ' NOTE!! Usually do not call takeStep directly. Add the action to actionManager.
'    object.runAction(action)
'End Sub
'
' 3.
'Private Sub testInstantAction()
'    Dim action As New CGActionInstant
'    action.instantAction = AddressOf actionContent
'    action.takeStep()
'End Sub
'
'Private Sub actionContent()
'    Console.WriteLine("(Subroutine or function) instant action executed!")
'End Sub

Public Class CGActionInstant : Inherits CGFiniteTimeAction
    Delegate Sub instantActionDelegate()
    Private instantAction_ As instantActionDelegate
    Public WriteOnly Property instantAction As instantActionDelegate
        Set(ByVal value As instantActionDelegate)
            instantAction_ = value
        End Set
    End Property

    Shared Function actionWithDelegate(ByVal actionDelegate As instantActionDelegate) As CGActionInstant
        Dim action As New CGActionInstant
        action.instantAction = actionDelegate
        Return action
    End Function

    Sub New(ByVal actionDelegate As instantActionDelegate)
        instantAction_ = actionDelegate
        duration = 0
    End Sub

    Sub New()
        instantAction_ = Nothing
        duration = 0
    End Sub

    Public Overrides Function Clone() As Object
        Dim copy As New CGActionInstant
        copy.instantAction = instantAction_.Clone
        Return copy
    End Function

    Public Overrides Function isDone() As Boolean
        Return True
    End Function

    Public Overrides Sub startWithTarget(ByVal target As Object)
        MyBase.startWithTarget(target)
        instantAction_.Invoke()
    End Sub

    Public Overrides Function reverse() As CGFiniteTimeAction
        Return CGActionInstant.actionWithDelegate(instantAction_.Clone)
    End Function

End Class
