Public Class Form1
    Private scene_ As CGScene

    Private Sub Form1_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Console.WriteLine("start testing")
        Dim director As CGDirector = CGDirector.sharedDirector
        director.canvas = PictureBox1
        director.mainWindow = Me
        director.animationInterval = 0.02
        scene_ = New CGScene
        director.runScene(scene_)
        testDirectorAndActions()
        'testDNAStrand()
        'testButtons()
        testSimpleGUI()
    End Sub

    'Private Sub Form1_Shown(sender As Object, e As EventArgs) Handles Me.Shown
    '    Me.FormBorderStyle = Windows.Forms.FormBorderStyle.None
    '    Me.Width = Screen.PrimaryScreen.Bounds.Width
    '    Me.Height = Screen.PrimaryScreen.Bounds.Height
    '    Me.CenterToScreen()
    'End Sub

    Private Sub testSimpleGUI()
        Dim simpleGUI As New CGSimpleGUILayer()
        Dim topHeight As Integer = 260
        Dim textView As New CGTextView(New RectangleF(0, topHeight, CGDirector.sharedDirector.canvasWidth, CGDirector.sharedDirector.canvasHeight - topHeight - simpleGUI.BottomBarHeight))
        Dim timeMachineLayer As New CGSprite(My.Resources.smile, New PointF(CGDirector.sharedDirector.canvasWidth / 2, CGDirector.sharedDirector.canvasHeight / 2))
        scene_.addChild(timeMachineLayer, kCGTopMostZOrder)
        timeMachineLayer.visible = False
        textView.parseFile("test.txt")
        scene_.addChild(textView)
        scene_.addChild(simpleGUI, kCGTopMostZOrder)
        simpleGUI.setClickHandlerOfButton(simpleGUI.rightButton, Sub(sender As Object, e As MouseEventArgs, info As Object)
                                                                     Console.WriteLine("click on right button")
                                                                     If CGDirector.sharedDirector.timeMachine.hasNextRecord Then
                                                                         timeMachineLayer.visible = True
                                                                         timeMachineLayer.texture = CGDirector.sharedDirector.timeMachine.nextRecord()
                                                                     Else
                                                                         If textView.hasNext() AndAlso Not timeMachineLayer.visible Then
                                                                             textView.showNextParagraph()
                                                                             If Not textView.hasNext() Then
                                                                                 simpleGUI.rightButton.setDisabled()
                                                                             End If
                                                                             CGDirector.sharedDirector.timeMachine.record()
                                                                             Console.WriteLine("   ===>   record")
                                                                         End If
                                                                         timeMachineLayer.visible = False
                                                                     End If
                                                                 End Sub)
        simpleGUI.setClickHandlerOfButton(simpleGUI.leftButton, Sub(sender As Object, e As MouseEventArgs, info As Object)
                                                                    Console.WriteLine("click on left button")
                                                                    If CGDirector.sharedDirector.timeMachine.hasPreviousRecord Then
                                                                        timeMachineLayer.visible = True
                                                                        timeMachineLayer.texture = CGDirector.sharedDirector.timeMachine.previousRecord()
                                                                    End If
                                                                End Sub)
    End Sub

    Private Sub testButtons()
        Dim button As New CGButton(New RectangleF(350, 400, 100, 40))
        button.title = "OK"
        scene_.addChild(button, kCGTopMostZOrder)
        'button.setDisabled()
        button.clickHandler = Sub(sender As CGButtonBase, e As MouseEventArgs, info As Object)
                                  Console.WriteLine(sender.ToString + " is clicked; click location at " + e.Location.ToString)
                              End Sub

        Dim button1 As New CGButtonSprite(My.Resources.right_normal, My.Resources.right_selected)
        button1.center = New PointF(CGDirector.sharedDirector.canvasWidth / 2, CGDirector.sharedDirector.canvasHeight / 2)
        scene_.addChild(button1, kCGTopMostZOrder)
        button1.clickHandler = Sub(sender As CGButtonBase, e As MouseEventArgs, info As Object)
                                   Console.WriteLine(sender.ToString + " is clicked; click location at " + e.Location.ToString)
                               End Sub

        Dim toggle As New CGButtonToggle(New Rectangle(200, 200, 100, 40))
        scene_.addChild(toggle, kCGTopMostZOrder)
        toggle.toggleHandler = Sub(sender As CGButtonBase, e As MouseEventArgs, info As Object)
                                   If info = True Then
                                       Console.WriteLine(sender.ToString + " is turned ON")
                                   Else
                                       Console.WriteLine(sender.ToString + " is turned OFF")
                                   End If
                               End Sub

        Dim toggle1 As New CGButtonToggleSprite(New PointF(350, 200),
                                                My.Resources.b_toggle_on, My.Resources.b_toggle_off,
                                                My.Resources.b_toggle_selected, My.Resources.b_toggle_offselected)
        scene_.addChild(toggle1, kCGTopMostZOrder)
        toggle1.toggleHandler = Sub(sender As CGButtonBase, e As MouseEventArgs, info As Object)
                                    If info = True Then
                                        Console.WriteLine(sender.ToString + " is turned ON")
                                    Else
                                        Console.WriteLine(sender.ToString + " is turned OFF")
                                    End If
                                End Sub
    End Sub

    Private Sub testDNAStrand()
        Dim singleStrand As New CGDNASingleStrand(New PointF(10, 10), 5, Color.Red, 10, 8)
        scene_.addChild(singleStrand)
        singleStrand.dyeDNAStrand(Color.Transparent, 2, 2)
        singleStrand.dyeDNAStrand(Color.Orange, 5, 5)
        Dim callback As CGActionInstant = CGActionInstant.actionWithDelegate(Sub()
                                                                                 singleStrand.moveHeadToDestination(100, New PointF(380, 200))
                                                                             End Sub)
        Dim spawn As New CGSpawn(callback, New CGDelayTime(100))
        Dim callback2 As CGActionInstant = CGActionInstant.actionWithDelegate(Sub()
                                                                                  singleStrand.moveEntireStrandByDistance(80, New PointF(50, 100))
                                                                              End Sub)
        Dim seq As New CGSequence(spawn, callback2)
        singleStrand.runAction(seq)
        'singleStrand.moveEntireStrandByDistance(80, New PointF(50, 200))

        Dim circleStrand As New CGDNASingleStrandCircular(New PointF(700, 400), 50, 5, Color.Orange, 15, 8)
        scene_.addChild(circleStrand)
        callback = CGActionInstant.actionWithDelegate(Sub()
                                                          circleStrand.moveHeadToDestination(120, New PointF(20, 10))
                                                      End Sub)
        spawn = New CGSpawn(callback, New CGDelayTime(120))
        seq = New CGSequence(New CGDelayTime(100), spawn)
        circleStrand.runAction(seq)

        Dim doubleStrand As New CGDNADoubleStrand(New PointF(400, 40), New PointF(400, 60), Color.Red, Color.Orange, 5, 20, 8)
        scene_.addChild(doubleStrand)
        For i As Integer = 0 To doubleStrand.strand1.count - 1
            Dim d1 As New CGDelayTime(i * 10)
            Dim index As Integer = i
            Dim unlink As CGActionInstant = CGActionInstant.actionWithDelegate(Sub()
                                                                                   doubleStrand.unlinkTwoNodesAtIndex(index)
                                                                               End Sub)
            Dim m As New CGMoveBy(20, New PointF(0, -30))
            'Dim link As CGActionInstant = CGActionInstant.actionWithDelegate(Sub()
            '                                                                     doubleStrand.linkTwoNodesAtIndex(index)
            '                                                                 End Sub)
            Dim link As New CGActionInstant(Sub()
                                                doubleStrand.linkTwoNodesAtIndex(index)
                                            End Sub)
            Dim d2 As New CGDelayTime((doubleStrand.strand1.count - 1 - i) * 10)
            Dim dmid As New CGDelayTime(15)
            seq = CGSequence.actionWithArray({d1, unlink, m, dmid, m.reverse, link, d2})
            Dim forever As New CGInfiniteTimeAction(seq)
            'doubleStrand.strand1.getDNANodeAtIndex(i).runAction(forever)
            seq = CGSequence.actionWithArray({d1, m.reverse, dmid, m, d2})
            forever = New CGInfiniteTimeAction(seq)
            'doubleStrand.strand2.getDNANodeAtIndex(i).runAction(forever)
        Next
    End Sub

    Private Sub testDirectorAndActions()
        'Dim label As New CGLabel("Test Text: Dim fff As New CGInfiniteTimeAction(CGSequence.actionWithArray({mmm, mmm.reverse}))", New RectangleF(0, 300, 700, 60))
        'label.textFont = New Font(kCGDefaultFontName, kCGDefaultFontSize)
        'Dim mmm As New CGMoveBy(90, New PointF(200, 0))
        'Dim fff As New CGInfiniteTimeAction(CGSequence.actionWithArray({mmm, mmm.reverse}))
        'scene_.addChild(label)
        'label.runAction(fff)

        Dim node As New CGDNANode(10, Color.Blue, New PointF(10, 10))
        scene_.addChild(node)
        Dim node2 As New CGDNANode(10, Color.Orange, New PointF(-10, 10))
        scene_.addChild(node2)
        Dim node3 As New CGDNANode(10, Color.Gold, New PointF(-30, 10))
        scene_.addChild(node3)
        Dim node4 As New CGDNANode(10, Color.Red, New PointF(-50, 10))
        scene_.addChild(node4)
        Dim mseq As CGSequence = CGSequence.actionWithArray({
                                                            New CGMoveBy(70, New PointF(350, 0)),
                                                            New CGMoveBy(45, New PointF(0, 200)),
                                                            New CGMoveBy(70, New PointF(-350, 0)),
                                                            New CGMoveBy(45, New PointF(0, -200))
                                                        })
        Dim mforever As New CGInfiniteTimeAction(mseq)
        node.runAction(mforever)
        node2.runAction(New CGFollow(node, 20))
        node3.runAction(New CGFollow(node2, 20))
        node4.runAction(New CGFollow(node3, 20))

        Dim smile As New CGSprite(My.Resources.smile, New PointF(80, 80))
        scene_.addChild(smile)
        Dim action As New CGMoveBy(80, New PointF(280, 130))
        Dim action2 As New CGFadeOut(80)
        Dim spawn As New CGSpawn(action, action2)
        Dim sequence As New CGSequence(spawn, spawn.reverse)
        Dim repeat As New CGInfiniteTimeAction(sequence)
        'Dim action As New CGActionInstant
        'action.instantAction = Sub() Console.WriteLine("say a word")
        'Dim repeat As New CGRepeat(action, 10)
        smile.runAction(repeat)

        Dim blinkSmile As New CGSprite(My.Resources.smile, New PointF(500, 130))
        scene_.addChild(blinkSmile)
        Dim blink As New CGBlink(15, 1)
        Dim forever As New CGInfiniteTimeAction(blink)
        blinkSmile.runAction(forever)

        Dim fadeIOSmile As New CGSprite(My.Resources.smile, New PointF(650, 130))
        scene_.addChild(fadeIOSmile)
        Dim fade As New CGFadeOut(20)
        Dim arr As New List(Of CGFiniteTimeAction)
        arr.Add(fade) : arr.Add(New CGDelayTime(10)) : arr.Add(fade.reverse)
        Dim sequence1 As CGSequence = CGSequence.actionWithArray(arr)
        Dim forever1 As New CGInfiniteTimeAction(sequence1)
        fadeIOSmile.runAction(forever1)

        Dim interactiveSmile As New CGSprite(My.Resources.smile, New PointF(500, 230))
        scene_.addChild(interactiveSmile)
        'Dim moveIA As New CGInteractionMoveTo(New Point(650, 230), True)
        Dim moveIA As New CGInteractionMoveBy(New Point(150, 0), True)
        moveIA.completionHandler = Sub(sender As Object, info As Object)
                                       Console.WriteLine(sender.ToString + "'s interaction is done")
                                   End Sub
        interactiveSmile.addInteraction(moveIA)

        Dim scaleSmile As New CGSprite(My.Resources.smile, New PointF(500, 40))
        scene_.addChild(scaleSmile)
        Dim scale As New CGScaleBy(15, -0.8)
        Dim scaleSeq As New CGSequence(scale, scale.reverse)
        Dim foreverScale As New CGInfiniteTimeAction(scaleSeq)
        scaleSmile.runAction(foreverScale)

        Dim rotateSmile As New CGSprite(My.Resources.smile, New PointF(650, 40))
        scene_.addChild(rotateSmile)
        Dim rotate As New CGRotateBy(40, 360)
        Dim foreverRotate As New CGInfiniteTimeAction(rotate)
        rotateSmile.runAction(foreverRotate)
    End Sub

    Private Sub testSchedule()
        Dim scheduler As New CGScheduler
        For i As Integer = 1 To 5
            scheduler.scheduleUpdate(New Object, 0, False)
        Next
        For i As Integer = 1 To 5
            scheduler.scheduleUpdate(New Object, -CInt(Math.Ceiling(Rnd() * 4)), False)
        Next
        For i As Integer = 1 To 5
            scheduler.scheduleUpdate(New Object, CInt(Math.Ceiling(Rnd() * 4)), False)
        Next
        Dim obj1 As New Object
        Dim obj2 As New Object
        Dim obj3 As New Object
        Dim obj4 As New Object
        Dim obj5 As New Object
        Dim obj6 As New Object
        scheduler.scheduleUpdate(obj1, 20, False)
        scheduler.scheduleUpdate(obj2, 19, False)
        scheduler.scheduleUpdate(obj3, -19, False)
        scheduler.scheduleUpdate(obj4, -20, False)
        scheduler.scheduleUpdate(obj5, 0, False)
        scheduler.scheduleUpdate(obj6, -18, False)
        scheduler.printAllScheduledUpdates()

        Console.WriteLine("Deletion...")
        scheduler.unscheduleUpdate(obj1)
        scheduler.unscheduleUpdate(obj2)
        scheduler.unscheduleUpdate(obj3)
        scheduler.unscheduleUpdate(obj4)
        scheduler.unscheduleUpdate(obj5)
        scheduler.unscheduleUpdate(obj6)
        scheduler.testRemoveUpdates()

        scheduler.printAllScheduledUpdates()
    End Sub

    Private Sub testInstantAction1()
        '        Dim action As New CGActionInstant
        '        ' pass lambda expression to instant action
        '        action.instantAction = Sub()
        '                                   Console.WriteLine("instant action executed!")
        '                               End Sub
        '        action.takeStep()
        Dim action As CGActionInstant = CGActionInstant.actionWithDelegate(Sub()
                                                                               Console.WriteLine("instant action executed!")
                                                                           End Sub)
        action.takeStep()
    End Sub

    Private Sub testInstantAction2()
        Dim action As New CGActionInstant
        action.instantAction = AddressOf actionContent
        action.takeStep()
    End Sub

    Private Sub actionContent()
        Console.WriteLine("(Subroutine or function) instant action executed!")
    End Sub


    'Private Sub deleteElementInArray()
    '    Dim arr As New List(Of CGAction)
    '    arr.Add(New CGAction)
    '    arr.Add(New CGAction)
    '    arr.Add(New CGAction)
    '    Dim action As New CGAction
    '    action.tag = 100
    '    arr.Add(action)
    '    arr.Add(New CGAction)
    '    arr.Add(New CGAction)
    '    Console.WriteLine("After add action, the number of actions is " + arr.Count.ToString)
    '    For Each a As CGAction In arr
    '        If a.tag = 100 Then
    '            arr.Remove(a)
    '            Exit For
    '        End If
    '    Next

    '    For i As Integer = 0 To arr.Count - 1
    '        If arr.Count > 0 Then
    '            arr.RemoveAt(i)
    '            Console.WriteLine(i.ToString)
    '            i -= 1
    '        End If
    '    Next
    'End Sub

    Private Sub testDynamicBinding()
        Dim a As Object = New CGNode
        a.location = New PointF(1, 2)
        Console.WriteLine("node a's center " + a.center.ToString)
        Dim b As New Object
        b.location = New PointF(1, 2)
        Console.WriteLine("node b's center " + b.center.ToString)
    End Sub

    Private Sub testScheduleNodeChildren()
        Dim father As New CGNode
        Dim child As New CGNode
        Dim childTagged As New CGNode
        Dim childZ As New CGNode
        childZ.tag = 4
        Dim child2 As New CGNode
        child2.tag = 2
        Dim child3 As New CGNode
        child3.tag = 3
        child.tag = 1
        childTagged.tag = 100
        father.addChild(child)
        father.addChild(child2)
        father.addChild(child3)
        father.addChild(childZ)
        father.addChild(childTagged)
        Console.WriteLine("After add child, the number of children is " + father.children.Count.ToString)
        Console.WriteLine("zOrder of a child is " + childZ.zOrder.ToString)
        childZ.zOrder = -10
        Console.WriteLine("After, zOrder of a child is " + childZ.zOrder.ToString)
        father.sortAllChildren()
        For Each node As CGNode In father.children
            Console.WriteLine(node.tag.ToString)
        Next
        father.removeChild(child, True)
        Console.WriteLine("After remove child, the number of children is " + father.children.Count.ToString)
        father.removeChildByTag(100, True)
        Console.WriteLine("After remove child by tag, the number of children is " + father.children.Count.ToString)
        father.removeAllChildren(True)
        Console.WriteLine("After remove all children, the number of children is " + father.children.Count.ToString)
    End Sub
End Class
